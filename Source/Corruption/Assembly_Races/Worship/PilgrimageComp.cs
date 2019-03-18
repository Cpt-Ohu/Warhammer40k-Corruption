using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace Corruption.Worship
{
    public class PilgrimageComp : WorldObjectComp
    {
        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
        }

        public Caravan caravan
        {
            get
            {
                return this.parent as Caravan;
            }
        }

        public static int PILGRIMAGE_TRAVELRADIUS = 20;

        public int OriginalMapTile;

        private int currentDestinationTile;
        
        private int ticksToTravel = 5000;

        private bool returningHome;

        private bool pilgrimageResolved;

        private float accumulatedSuccessChange = 0f;

        private PatronDef pilgrimGod;

        private bool pilgrimageActive;

        private void CheckPilgrimageActive()
        {
            IEnumerable<Pawn> colonists = (caravan.pawns.InnerListForReading.Where(x => CompSoul.GetPawnSoul(x) != null));
            this.pilgrimageActive = colonists.Any(p => CompSoul.GetPawnSoul(p).IsOnPilgrimage);


            if (this.pilgrimageActive)
            {
                foreach (Pawn p in caravan.PawnsListForReading)
                {
                    p.SetFactionDirect(CFind.StoryTracker.IoM_NPC);
                }
                this.caravan.SetFaction(CFind.StoryTracker.IoM_NPC);
                this.currentDestinationTile = this.caravan.pather.Destination;
                this.ticksToTravel = Rand.Range(180000, 420000);
                Messages.Message("PilgrimageStartedMessage".Translate((int)(this.ticksToTravel / GenDate.TicksPerDay)), MessageTypeDefOf.PositiveEvent);
            }
            this.pilgrimageResolved = true;

        }

        public override void CompTick()
        {
            if (!this.pilgrimageResolved)
            {
                CheckPilgrimageActive();
            }
            base.CompTick();
            if (!this.returningHome)
            {
                this.ticksToTravel--;
                if (this.parent.Tile == currentDestinationTile)
                {
                    this.PilgrimageDecisionPoint();
                }
            }
        }

        private void PilgrimageDecisionPoint()
        {

            if (ticksToTravel <= 0)
            {
                this.ReturnHome();
            }
            else
            {
                this.SetRandomDestination();
            }
        }
        
        private void UpdatePilgrimageInventory()
        {
            string text;
            if (this.caravan.needs.AnyPawnOutOfFood(out text))
            {
                if (Rand.Value > 0.5f)
                {
                    ThingDef eatableDef = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsNutritionGivingIngestible).RandomElement();
                    Thing food = ThingMaker.MakeThing(eatableDef);
                    food.stackCount = eatableDef.stackLimit;
                    this.caravan?.AddPawnOrItem(food, false);
                    Messages.Message("PilgrimsForagedFood".Translate(), this.caravan, MessageTypeDefOf.PositiveEvent);
                }
            }

            if (Rand.Value > accumulatedSuccessChange)
            {
                if (Rand.Value > 0.2f)
                {
                    ThingDef relicDef = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasComp(typeof(CompSoulItem))).RandomElementByWeight(y => y.GetCompProperties<CompProperties_SoulItem>().DedicatedGod == this.pilgrimGod ? 1 : 0.1f);
                    Thing newRelic = ThingMaker.MakeThing(relicDef);
                    this.caravan?.AddPawnOrItem(newRelic, false);
                }
                else
                {
                    Faction RandomFaction = Find.World.factionManager.AllFactions.Where(x => !x.HostileTo(Faction.OfPlayer) && x.def.basicMemberKind?.race == Faction.OfPlayer.def.basicMemberKind.race).RandomElement();
                    Pawn newFollower = PawnGenerator.GeneratePawn(RandomFaction.RandomPawnKind(), CFind.StoryTracker.IoM_NPC);
                    CompSoul newSoul = CompSoul.GetPawnSoul(newFollower);
                    if (newFollower.skills.GetSkill(SkillDefOf.Social).Level < this.caravan?.pawns.InnerListForReading.Max(x => x.skills.GetSkill(SkillDefOf.Social).Level) + Rand.Range(-2, 2))
                    {
                        newSoul.GainChaosGod(this.pilgrimGod);
                    }

                    Find.WorldPawns.PassToWorld(newFollower);
                    this.caravan?.AddPawn(newFollower, true);
                }
            }
            else
            {
                this.accumulatedSuccessChange += 0.1f;
            }
        }

        private void SetRandomDestination()
        {
            this.UpdatePilgrimageInventory();
            int tile;
            TileFinder.TryFindPassableTileWithTraversalDistance(this.parent.Tile, 15, 50, out tile, (int x) => Find.WorldGrid[x].biome.canAutoChoose, true, true);
            this.currentDestinationTile = tile;
            Log.Message($"Trying random destination from {this.parent.Tile} to {tile}");
            this.caravan?.pather.StartPath(tile, null, true);
        }

        public void ForceReturnHome()
        {
            this.ReturnHome();
        }

        public Gizmo ReturnHomeCommand()
        {
            Command_Action command = new Command_Action();
            command.defaultLabel = "CommandPilgrimageReturnHome".Translate();
            command.defaultDesc = "CommandPilgrimageReturnHomeDesc".Translate();
            command.icon = SettleUtility.SettleCommandTex;
            command.action = delegate
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                this.ForceReturnHome();
            };
            return command;
        }

        private void ReturnHome()
        {
            this.UpdatePilgrimageInventory();
            this.returningHome = true;
            List<Pawn> Pawns = this.caravan?.PawnsListForReading.FindAll(x => x.RaceProps.Humanlike);


            foreach (Pawn p in caravan?.PawnsListForReading)
            {
                p.SetFactionDirect(Faction.OfPlayer);

                CompSoul soul = CompSoul.GetPawnSoul(p);
                if (soul != null)
                {
                    soul.IsOnPilgrimage = false;
                }
            }
            this.caravan?.SetFaction(Faction.OfPlayer);
            this.caravan?.pather.StartPath(this.OriginalMapTile, new CaravanArrivalAction_Enter(Find.World.worldObjects.MapParentAt(this.OriginalMapTile)));

            CFind.WorshipTracker.AddWorshipProgress(1500, CFind.WorshipTracker.PlayerGod);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToTravel, "ticksToTravel", 2500);
            Scribe_Values.Look<int>(ref this.OriginalMapTile, "ticksToTravel", 2500);
            Scribe_Values.Look<int>(ref this.currentDestinationTile, "currentDestinationTile", 2500);
            Scribe_Values.Look<bool>(ref this.returningHome, "returningHome", false);
            Scribe_Values.Look<bool>(ref this.pilgrimageResolved, "pilgrimageResolved", false);
            Scribe_Defs.Look<PatronDef>(ref this.pilgrimGod, "pilgrimGod");

        }
    }
}
