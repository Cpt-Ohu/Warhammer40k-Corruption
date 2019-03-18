using Corruption.Domination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public static class CFind
    {
        private static CorruptionStoryTracker tracker;

        public static CorruptionStoryTracker StoryTracker
        {
            get
            {
                if (tracker == null)
                {
                    tracker = Find.World.GetComponent<CorruptionStoryTracker>();
                }
                return tracker;
            }
        }


        public static Missions.MissionManager MissionManager
        {
            get
            {
                return CFind.StoryTracker?.MissionManager;
            }
        }

        private static DominationTracker dominationTracker;

        public static DominationTracker DominationTracker
        {
            get
            {
                return CFind.StoryTracker?.DominationTracker;
            }
        }

        public static Worship.WorshipTracker WorshipTracker
        {
            get
            {
                return StoryTracker.WorshipTracker;
            }
        }
    }
}
