using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

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

        private int travelledTicks;

        private int ticksToTravel;

        private bool returningHome;

        private bool pilgrimageResolved;

        private float accumulatedSuccessChange = 0f;

        private PatronDef pilgrimGod;

        private bool pilgrimageActive;

        private void CheckPilgrimageActive()
        {
            IEnumerable<Pawn> colonists = (caravan.pawns.InnerListForReading.Where(x => CompSoul.GetPawnSoul(x) != null));
            foreach (Pawn p in colonists)
            {
                if(CompSoul.GetPawnSoul(p).IsOnPilgrimage)
                {
                    this.pilgrimageActive = true;
                    this.caravan.SetFaction(CorruptionStoryTrackerUtilities.CurrentStoryTracker.IoM_NPC);
                    break;
                }
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
                if (this.parent.Tile == currentDestinationTile)
                {
                    this.PilgrimageDecisionPoint();
                }
            }
        }
        
        private void PilgrimageDecisionPoint()
        {

            if (this.travelledTicks >= ticksToTravel)
            {
                this.ReturnHome();
                return;
            }
            else
            {
                this.SetRandomDestination();
            }

        }

        private void UpdatePilgrimageInventory()
        {
            string text;
            if (CaravanPawnsNeedsUtility.AnyPawnOutOfFood(this.caravan, out text))
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
                    ThingDef relicDef = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasComp(typeof(CompSoulItem))).RandomElementByWeight(y => y.GetCompProperties<CompProperties_SoulItem>().DedicatedGod == this.pilgrimGod ? 1 : 0);
                    Thing newRelic = ThingMaker.MakeThing(relicDef);
                    this.caravan?.AddPawnOrItem(newRelic, false);
                }
                else
                {
                    Faction RandomFaction = Find.World.factionManager.AllFactions.Where(x => !x.HostileTo(Faction.OfPlayer) && x.def.basicMemberKind?.race == Faction.OfPlayer.def.basicMemberKind.race).RandomElement();
                    Pawn newFollower = PawnGenerator.GeneratePawn(RandomFaction.RandomPawnKind());
                    CompSoul newSoul = CompSoul.GetPawnSoul(newFollower);
                    if (newFollower.skills.GetSkill(SkillDefOf.Social).Level < this.caravan?.pawns.InnerListForReading.Max(x => x.skills.GetSkill(SkillDefOf.Social).Level) + Rand.Range(-2, 2))
                    {
                        newSoul.GainChaosGod(this.pilgrimGod);
                    }

                    this.caravan?.AddPawn(newFollower, true);
                }
            }
        }

        private void SetRandomDestination()
        {
            int tile;
            TileFinder.TryFindPassableTileWithTraversalDistance(this.parent.Tile, 5, 800, out tile, (int x) => Find.WorldGrid[x].biome.canAutoChoose, true, true);
            this.currentDestinationTile = tile;
            this.caravan?.pather.StartPath(tile, null, true);
        }

        public void ForceReturnHome()
        {
            this.ReturnHome();
        }

        private void ReturnHome()
        {
            this.caravan?.pather.StartPath(this.OriginalMapTile, new CaravanArrivalAction_Enter(Find.World.worldObjects.MapParentAt(this.OriginalMapTile)));
            this.returningHome = true;
            this.caravan?.SetFaction(Faction.OfPlayer);
            List<Pawn> Pawns = this.caravan?.PawnsListForReading.FindAll(x => x.RaceProps.Humanlike);
            for (int i = 0; i < Pawns.Count; i++)
            {
                CompSoul soul = CompSoul.GetPawnSoul(Pawns[i]);
                if (soul != null)
                {
                    soul.IsOnPilgrimage = false;
                }
            }
        }
        
        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToTravel, "ticksToTravel", 2500);
            Scribe_Values.Look<int>(ref this.travelledTicks, "travelledTicks", 0);
            Scribe_Values.Look<int>(ref this.OriginalMapTile, "ticksToTravel", 2500);
            Scribe_Values.Look<int>(ref this.currentDestinationTile, "currentDestinationTile", 2500);
            Scribe_Values.Look<bool>(ref this.returningHome, "returningHome", false);
            Scribe_Values.Look<bool>(ref this.pilgrimageResolved, "pilgrimageResolved", false);
            Scribe_Defs.Look<PatronDef>(ref this.pilgrimGod, "pilgrimGod");

        }
    }
}
