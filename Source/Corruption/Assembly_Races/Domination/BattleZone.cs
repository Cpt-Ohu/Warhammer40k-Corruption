using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Corruption.Domination
{
    public class BattleZone : MapParent, IBattleZone
    {
        public List<Faction> WarringFactions = new List<Faction>();
        
        public Faction PlayerChosen;

        public Faction DefendingFaction;

        private string battleName;

        public BattleSize BattleSize;

        public BattleType BattleType;

        private bool battleResolved = false;

        public Faction winningFaction;

        public int[] battlePointRange = new int[2] { 0, 0 };

        public BattleZone()
        {
            this.BattleSize = (BattleSize)Rand.RangeInclusive(0, 2);
        }

        public override string Label
        {
            get
            {
                return this.battleName;
            }
        }              

        public override string GetInspectString()
        {

            StringBuilder stringBuilder = new StringBuilder();
            string desc = "";
            for (int i =0; i< this.WarringFactions.Count; i++)
            {
                desc += WarringFactions[i].Name + " vs. ";
            }
            stringBuilder.Append(desc);
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }

        public void InitializeBattle(BattleSize battleSize, BattleType battleType, List<Faction> participatingFactions, string battleNameRulePack, Faction defendingFaction = null)
        {
            if (battleSize == BattleSize.Random)
            {
                this.BattleSize = (BattleSize)Rand.RangeInclusive(0, 2);
            }
            else
            {
                this.BattleSize = battleSize;
            }

            this.BattleType = battleType;

            if (this.BattleType == BattleType.CampSiege)
            {
                if (defendingFaction == null)
                {
                    this.DefendingFaction = defendingFaction;
                }
                else
                {
                    this.DefendingFaction = participatingFactions.RandomElement();
                }
            }

            foreach (Faction current in participatingFactions)
            {
                this.WarringFactions.Add(current);
            }

            this.battleName = NameGenerator.GenerateName(RulePackDef.Named(battleNameRulePack));
        }

        public string BattleName
        {
            get
            {
                return this.battleName;
            }
        }           

        public override void PostMake()
        {
            base.PostMake();
            this.SetBattlePoints();            
        }

        private void SetBattlePoints()
        {
            switch(this.BattleSize)
            {
                case BattleSize.Small:
                    {
                        this.battlePointRange = new int[2] { 300, 600 };
                        break; 
                    }
                case BattleSize.Medium:
                    {
                        this.battlePointRange = new int[2] { 1000, 1500 };
                        break;
                    }
                case BattleSize.Large:
                    {
                        this.battlePointRange = new int[2] { 2000, 3000 };
                        break;
                    }
            }
        }
        
        public bool playerIsParticipating
        {
            get
            {
                return this.PlayerChosen != null;
            }
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = true;
            return this.battleResolved;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            string comtitle = "JoinBattle".Translate();
            string comdesc = "JoinBattleDesc".Translate();
            Command_Action command_join = new Command_Action();
            command_join.defaultLabel = comtitle;
            command_join.defaultDesc = comdesc;
            command_join.icon = CorruptionStoryTrackerUtilities.JoinBattle;
            command_join.action = delegate
            {
                SoundDefOf.TickHigh.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_ChooseBattleSides(this));
            };
            yield return command_join;
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
        }

        public bool StartResolving()
        {
            List<IAttackTarget> acticeAdversaries = new List<IAttackTarget>();
            for (int i = 0; i < this.WarringFactions.Count; i++)
            {
                acticeAdversaries.Clear();
                acticeAdversaries.AddRange(Map.attackTargetsCache.TargetsHostileToFaction(this.WarringFactions[i]));
                if (acticeAdversaries.Count < 1 || !acticeAdversaries.Any(x => GenHostility.IsActiveThreat(x)))
                {
                    this.winningFaction = this.WarringFactions[i];
                    this.battleResolved = true;
                    return true;
                }
            }
            return false;
        }

        public void GenerateMap()
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                IntVec3 vec3;
                if (2 > 1)
                {
                    vec3 = Find.World.info.initialMapSize;
                }
                else
                {
                   // vec3 = new IntVec3(100, 1, 100);
                }
                Map visibleMap = MapGenerator.GenerateMap(vec3, this, DefOfs.C_MapGeneratorDefOf.MapGeneratorBattleZone, null, null);
                Current.Game.VisibleMap = visibleMap;
            }, "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap));
            LongEventHandler.QueueLongEvent(delegate
            {
                Map map = this.Map;
                Find.CameraDriver.JumpToVisibleMapLoc(map.Center);
                Find.MainTabsRoot.EscapeCurrentTab(false);
            }, "SpawningColonists", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap));

            foreach (Faction current in this.WarringFactions)
            {
                if (this.PlayerChosen == Faction.OfPlayer)
                {
                    current.SetHostileTo(Faction.OfPlayer, true);
                }
                else if (this.PlayerChosen != current && this.PlayerChosen == null)
                {
                    current.SetHostileTo(Faction.OfPlayer, true);
                }
            }
        }
    }
}
