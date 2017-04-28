using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Injector40K;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;
using System.Threading;

namespace AlienRace
{
    public class ModInitializer : ITab
    {
        protected GameObject modInitializerControllerObject;

        public ModInitializer()
        {
            Log.Message("Initialized AlienRace Mod");
            this.modInitializerControllerObject = new GameObject("AlienRacer");
            this.modInitializerControllerObject.AddComponent<ModInitializerBehaviour>();
            this.modInitializerControllerObject.AddComponent<DoOnMainThread>();
            UnityEngine.Object.DontDestroyOnLoad(this.modInitializerControllerObject);
        }

        protected override void FillTab()
        {}
    }
	
public class DoOnMainThread : MonoBehaviour
    {

        public static readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

        public void Update()
        {
            while (ExecuteOnMainThread.Count > 0)
            {
                ExecuteOnMainThread.Dequeue().Invoke();
            }
        }
    }



    class ModInitializerBehaviour : MonoBehaviour
    {

     
        public void FixedUpdate()
        {
        }

        public void OnLevelWasLoaded()
        {
        }

        public void Start()
        {
            Log.Message("Initiated Alien Pawn Detours.");
            MethodInfo method1a = typeof(Verse.GenSpawn).GetMethod("Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Rot4) });
            MethodInfo method1b = typeof(GenSpawnAlien).GetMethod("SpawnModded", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Rot4) });

            MethodInfo method2a = typeof(RimWorld.InteractionWorker_RecruitAttempt).GetMethod("DoRecruit", new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(bool) });
            MethodInfo method2b = typeof(AlienRace.AlienRaceUtilities).GetMethod("DoRecruitAlien", new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(bool) });

            try
            {                
                Detours.TryDetourFromTo(method1a, method1b);
                Detours.TryDetourFromTo(method2a, method2b);
                Log.Message("Spawn method detoured!");
            }
            catch (Exception)
            {
                Log.Error("Could not detour Alien graphics");
                throw;
            }
        }
    }
}
