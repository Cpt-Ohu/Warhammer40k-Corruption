using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Domination
{
    public interface IBattleZone : ILoadReferenceable
    {
        void InitializeBattle(BattleSize battleSize, BattleType battleType, List<Faction> participatingFactions, string battleNameRulePack, Faction defendingFaction = null);

        string BattleName
        {
            get;
        }

        bool playerIsParticipating
        {
            get;
        }

        bool StartResolving();

    }
}
