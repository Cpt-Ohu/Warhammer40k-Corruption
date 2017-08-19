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
        public List<PoliticalAlliance> WarringAlliances = new List<PoliticalAlliance>();
        
        public PoliticalAlliance PlayerChosen;

        public PoliticalAlliance DefendingFaction;

        public BattleResult Result;

        private string battleName;

        public BattleSize BattleSize;

        public BattleType BattleType;

        private bool battleResolved = false;

        public PoliticalAlliance winningFaction;                

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

        public override MapGeneratorDef MapGeneratorDef
        {
            get
            {
                return DefOfs.C_MapGeneratorDefOf.MapGeneratorBattleZone;
            }
        }

        public override string GetInspectString()
        {

            StringBuilder stringBuilder = new StringBuilder();
            string desc = "";
            for (int i =0; i< this.WarringAlliances.Count; i++)
            {
                desc += WarringAlliances[i].AllianceName + (i < this.WarringAlliances.Count -1 ? " vs. " : "");
            }
            stringBuilder.Append(desc);
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }

        public void InitializeBattle(BattleSize battleSize, BattleType battleType, List<PoliticalAlliance> participatingAlliance, string battleNameRulePack, PoliticalAlliance defendingFaction = null)
        {

            this.Result = new BattleResult();
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
                    this.DefendingFaction = participatingAlliance.RandomElement();
                }
            }

            foreach (PoliticalAlliance current in participatingAlliance)
            {
                this.WarringAlliances.Add(current);
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
            if (this.battleResolved)
            {
                if (this.EnemiesRouted)
                {
                    return true;
                }
            }
            return false;
        }

        private bool EnemiesRouted
        {
            get
            {
                foreach (Faction fac in this.Result.Loser.GetFactions())
                {
                    if (!this.Map.mapPawns.AllPawns.Any(x => (x.Faction == fac && (!x.Downed || x.Dead)) || x.Faction == Faction.OfPlayer))
                    {
                        return true;
                    }
                }
                return false;
            }
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

        public override void Tick()
        {
            base.Tick();
            if (!this.battleResolved && this.HasMap)
            {
                this.CheckBattle();
            }
        }

        private void CheckBattle()
        {
            if (Find.TickManager.TicksGame % 205 == 0)
            {
                this.StartResolving();
            }
        }

        public bool StartResolving()
        {
            for (int i = 0; i < this.WarringAlliances.Count; i++)
            {
                int adversaryCount = 0;
                foreach (Faction current in this.WarringAlliances[i].GetFactions())
                {
                    adversaryCount += DominationUtilities.ActiveFightersFor(current, this.Map);
                }
                if (adversaryCount < 1)
                {
                    //Log.Message("FOund Winner: " + this.WarringAlliances[i].AllianceName);
                    this.winningFaction = this.WarringAlliances[i];
                    this.battleResolved = true;
                }
            }
            if (this.battleResolved)
            {
                PoliticalAlliance loserFaction = this.WarringAlliances.FirstOrDefault(x => x != this.winningFaction);
                //Log.Message("Losers: " + loserFaction.AllianceName);
                this.Result.ResolveFactions(this.winningFaction, loserFaction, this.Map);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
        }

        public void GenerateMap()
        {
            Map newMap;
            LongEventHandler.QueueLongEvent(delegate
            {
                newMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.Tile, Find.World.info.initialMapSize, this.def);
                newMap.info.parent = this;
                Find.CameraDriver.JumpToVisibleMapLoc(newMap.Center);
            }, "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap));
            
            //LongEventHandler.QueueLongEvent(delegate
            //{
            //    Map map = this.Map;
            //    Find.CameraDriver.JumpToVisibleMapLoc(map.Center);
            //    Find.MainTabsRoot.EscapeCurrentTab(false);
            //}, "SpawningColonists", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap));
        }

        private void ResolvePlayerDecision()
        {
            foreach (PoliticalAlliance current in this.WarringAlliances)
            {
                if (this.PlayerChosen != current && this.PlayerChosen == null)
                {
                    current.SetHostileTo(CorruptionStoryTrackerUtilities.currentStoryTracker.DominationTracker.PlayerAlliance);
                }
            }
        }
    }
}
