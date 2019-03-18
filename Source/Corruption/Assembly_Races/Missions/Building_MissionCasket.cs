using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Corruption.Missions
{
    [StaticConstructorOnStartup]
    public class Building_MissionCasket : Building_Casket
    {
        public Building_MissionCasket()
        {
        }

        public override void PostMake()
        {
            base.PostMake();

            CompQuality compQuality = this.GetComp<CompQuality>();
            compQuality?.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
        }

        private static Texture2D PlaceBeacon = ContentFinder<Texture2D>.Get("UI/Commands/PlaceBeacon", true);

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }


            yield return new Command_Action()
            {
                defaultDesc = "PlaceBeacon".Translate(),
                defaultLabel = "PlaceBeaconDesc".Translate(),
                action = delegate
                {
                    this.DeSpawn();
                    SoundDef.Named("CryptosleepCasketEject").PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));

                    CFind.MissionManager.TryFinishMissionForTarget(this);
                }
            };

            //if (base.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && this.def.building.isPlayerEjectable)
            //{
            //    Command_Action eject = new Command_Action();
            //    eject.action = new Action(this.EjectContents);
            //    eject.defaultLabel = "CommandPodEject".Translate();
            //    eject.defaultDesc = "CommandPodEjectDesc".Translate();
            //    if (this.innerContainer.Count == 0)
            //    {
            //        eject.Disable("CommandPodEjectFailEmpty".Translate());
            //    }
            //    eject.hotKey = KeyBindingDefOf.Misc1;
            //    eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
            //    yield return eject;
            //}
        }

        public override void EjectContents()
        {
            ThingDef filthSlime = ThingDefOf.Filth_Slime;


            CFind.MissionManager.TryFailMissionForTarget(this);

            if (!base.Destroyed)
            {
                SoundDef.Named("CryptosleepCasketEject").PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }
            base.EjectContents();
        }

    }
}
