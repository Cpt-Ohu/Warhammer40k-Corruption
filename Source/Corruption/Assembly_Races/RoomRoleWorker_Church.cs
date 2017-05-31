using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
        public class RoomRoleWorker_Church : RoomRoleWorker
        {
            public override float GetScore(Room room)
            {
                int num = 0;
                List<Thing> allContainedThings = room.ContainedAndAdjacentThings;
                for (int i = 0; i < allContainedThings.Count; i++)
                {
                    Thing thing = allContainedThings[i];
                    BuildingAltar altar = thing as BuildingAltar;
                    if (altar != null && altar.def.size.x > 1)
                    {
                        num+=20;
                    }
                    else if (altar != null && altar.def.size.x == 1)
                    if (altar != null && altar.def.size.x > 1)
                    {
                        num += 5;
                    }
            }
                if (num >= 20)
                {
                    return 10000;
                }
                return 0f;
            }
    }    
}
