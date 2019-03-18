using Corruption.Domination;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace Corruption.Domination
{
    public static class FactionDialogMakerDiplomacy
    {
        private const float NEGOTIATE_SUBJUGATION_BASE_CHANCE = 0.1f;
        private const float NEGOTIATE_TRUCE_BASE_CHANCE = 0.3f;
        private const float NEGOTIATE_PEACE_BASE_CHANCE = 0.2f;
        private const float NEGOTIATE_LEADER_FAVOUR = 0.2f;

        public static DiaOption GetDiplomacyOptionsFor(Faction faction, Pawn negotiator)
        {
            Domination.PoliticalAlliance npcAlliance = CFind.DominationTracker.GetAllianceOfFaction(faction);
            DiaOption diaOption = new DiaOption("PoliticsNode".Translate());

            diaOption.link = AllianceWelcomeNode(npcAlliance, faction, negotiator);
            return diaOption;
        }

        private static DiaOption JoinAllianceNPC(PoliticalAlliance npcAlliance, Faction faction, Pawn negotiator)
        {
            return new DiaOption("JoinAllianceNPC".Translate())
            {
                link = new DiaNode("JoinAllianceNPCDesc".Translate())
                {
                    options = {
                        new DiaOption("SwearAllegiance".Translate())
                    {
                        action = delegate
                            {
                                var previousAlliance = CFind.DominationTracker.PlayerAlliance;
                                previousAlliance.RemoveFromAlliance(Faction.OfPlayer, false);
                                npcAlliance.AddToAlliance(Faction.OfPlayer);
                            },
                        linkLateBind = () => FactionDialogMakerDiplomacy.AllianceWelcomeNode(npcAlliance, faction, negotiator)
                    }
                    }
                }
            };        
        }


        private static DiaOption BackToAlliance(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            return new DiaOption("Back".Translate())
            {
                linkLateBind = () => FactionDialogMakerDiplomacy.AllianceWelcomeNode(alliance, faction, negotiator)
            };
        }

        private static DiaNode AllianceWelcomeNode(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            if (alliance.HostileTo(CFind.DominationTracker.PlayerAlliance))
            {
                return FactionDialogMakerDiplomacy.HostileAllianceNode(alliance, faction, negotiator);
            }
            else if (alliance.IsPlayerAlliance && alliance.LeadingFaction == Faction.OfPlayer)
            {
                return FactionDialogMakerDiplomacy.PlayerAllianceNPCMemberNode(alliance, faction, negotiator);
            }
            else if (alliance.IsPlayerAlliance && alliance.LeadingFaction != Faction.OfPlayer)
            {
                return FactionDialogMakerDiplomacy.PlayerAllianceLeaderNode(alliance, faction, negotiator);
            }
            else
            {
                return FactionDialogMakerDiplomacy.NeutralAllianceNode(alliance, faction, negotiator);
            }
        }

        private static DiaNode NeutralAllianceNode(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            DiaNode node = new DiaNode("AlliancePoliticsNeutral".Translate());
            node.options.Add(JoinAllianceNPC(alliance, faction, negotiator));
            node.options.Add(OfferAllianceNPC(alliance, faction, negotiator));
            node.options.Add(DeclareWar(alliance, faction, negotiator));
            node.options.Add(BackToRoot(faction, negotiator));
            return node;
        }

        private static DiaOption DeclareWar(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            return new DiaOption("DeclareWar".Translate())
            {
                action = delegate
                {
                    CFind.DominationTracker.AddPlayerConflictWith(alliance);
                },
                link = new DiaNode("DeclaredWarDesc".Translate())
                {
                    options =
                    {
                        OKToRoot(faction, negotiator)
                    }
                }
            };
        }

        private static DiaOption OfferAllianceNPC(PoliticalAlliance alliance, Faction faction, Pawn negotiatior)
        {
            DiaOption diaOption = new DiaOption("OfferAllianceNPC".Translate());
            float rawPower = alliance.CombinedPowerAgainst(alliance);

            DominationConflict conflict = CFind.DominationTracker.AllConflicts.FirstOrDefault(x => x.IsWarParty(alliance) && x.IsWarParty(CFind.DominationTracker.PlayerAlliance));
            if (conflict != null)
            {
                var disabledOption = new DiaOption("OfferAllianceNPC".Translate());
                disabledOption.Disable("AtWar".Translate());
                return disabledOption;
            }

            return diaOption;
        }

        private static DiaNode PlayerAllianceLeaderNode(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            float negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).Level;
            DiaNode node = new DiaNode("AlliancePoliticsLeader".Translate(alliance.AllianceName, negotiator.Name.ToStringShort));
            node.options.Add(RequestMissionFrom(alliance, faction, negotiator));
            node.options.Add(ChallengeLeadership(alliance, faction, negotiator));
            node.options.Add(RequestLowerTaxes(alliance, faction, negotiator));
            node.options.Add(RequestLowerTributes(alliance, faction, negotiator));
            node.options.Add(BackToRoot(faction, negotiator));
            return node;
        }

        private static DiaOption ChallengeLeadership(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            float negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).Level / 40f;
            float playerPower = Find.Maps.Where(x => x.IsPlayerHome).Sum(x => x.PlayerWealthForStoryteller);
            float relationGoodwill = faction.RelationWith(Faction.OfPlayer).goodwill / 200f;
            var diaOption = new DiaOption("ChallengeLeadership".Translate(negotiatorSkill * 100));
            if (Rand.Value < negotiatorSkill + relationGoodwill)
            {
                diaOption.action = delegate
                {
                    alliance.LeadingFaction = Faction.OfPlayer;
                };

                diaOption.link = new DiaNode("AllowPlayerBecomeLeader".Translate())
                {
                    options =
                    {
                        BackToRoot(faction, negotiator)
                    }
                };
            }
            else if (Rand.Value > 0.3f - relationGoodwill)
            {
                diaOption.action = delegate
                {
                    alliance.RemoveFromAlliance(Faction.OfPlayer, true);
                };
                diaOption.link = new DiaNode("ExpelPlayerFromAlliance".Translate())
                {
                    options =
                    {
                        BackToRoot(faction, negotiator)
                    }
                };
            }
            else
            {
                diaOption.action = delegate
                {
                    alliance.RemoveFromAlliance(Faction.OfPlayer, true);
                };
                diaOption.link = new DiaNode("PlayerChallengedLeadershipFailed".Translate())
                {
                    options =
                    {
                        BackToRoot(faction, negotiator)
                    }
                };
            }
            return diaOption;
        }

        private static DiaOption RequestLowerTaxes(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            float negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).Level / 20f;
            float relationGoodwill = faction.RelationWith(Faction.OfPlayer).goodwill / 200f;
            return new DiaOption("RequestLowerTaxes".Translate(Math.Min(100, (negotiatorSkill + relationGoodwill) * 100)))
            {
                linkLateBind = ResetToRoot(faction, negotiator)
            };
        }
        
        private static DiaOption RequestLowerTributes(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            var negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).Level / 30f;
            float chance = negotiatorSkill + NEGOTIATE_LEADER_FAVOUR;
            var diaOption = new DiaOption("RequestLowerTribute".Translate(chance));

            if (Rand.Value < chance)
            {
                diaOption.action = delegate
                {
                    CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(faction, Faction.OfPlayer, -5);
                };
                diaOption.link = new DiaNode("AcceptPlayerFavour".Translate())
                {
                    options =
                    {
                        BackToAlliance(alliance, faction, negotiator)
                    }
                };
            }
            else
            {
                diaOption.action = delegate
                {
                    CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(faction, Faction.OfPlayer, -15);
                };
                diaOption.link = new DiaNode("DeclinePlayerFavour".Translate())
                {
                    options =
                    {
                        BackToAlliance(alliance, faction, negotiator)
                    }
                };
            }
            return diaOption;
        }

        private static DiaOption RequestMissionFrom(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            return new DiaOption("RequestMission".Translate())
            {
                linkLateBind = ResetToRoot(faction, negotiator)
            };
        }

        private static DiaNode HostileAllianceNode(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            var result = new DiaNode("AlliancePoliticsHostile".Translate(alliance.AllianceName));
            var negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).Level / 20f;
            float openConflictFactor = 0f;
            DominationConflict conflict = CFind.DominationTracker.AllConflicts.FirstOrDefault(x => x.IsWarParty(alliance) && x.IsWarParty(CFind.DominationTracker.PlayerAlliance));
            if (conflict != null)
            {
                openConflictFactor = conflict.GetWarEffort(alliance).WarWeariness;

                if (openConflictFactor + negotiatorSkill * 0.5f > 1f)
                {
                    result.options.Add(OfferPeace(alliance, faction, negotiator, conflict));
                }
                if (negotiatorSkill > 0.5f)
                {
                    result.options.Add(DemandSubjugation(alliance, faction, negotiator, conflict));
                }
                if (conflict.TruceActive == false && conflict.GetWarEffort(alliance).WarWeariness > 0.2f)
                {
                    result.options.Add(OfferTruce(alliance, faction, negotiator, conflict));
                }
            }
            else if (CFind.DominationTracker.PlayerAlliance.PlayerIsLeader)
            {
                result.options.Add(DeclareWar(alliance, faction, negotiator));
            }
            result.options.Add(BackToRoot(faction, negotiator));
            return result;

        }

        private static DiaOption OfferTruce(PoliticalAlliance alliance, Faction faction, Pawn negotiator, DominationConflict conflict)
        {
            int negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).levelInt;
            float chance = CalculateNegotiationChance(NEGOTIATE_SUBJUGATION_BASE_CHANCE, alliance, CFind.DominationTracker.PlayerAlliance, negotiatorSkill);
            var truceOption = new DiaOption("OfferTruce".Translate(chance * 100));
            truceOption.linkLateBind = ResetToRoot(faction, negotiator);
            if (Rand.Value < chance)
            {
                truceOption.action = delegate
                {
                    conflict.ActivateTruce();
                };
            }
            else
            {
                truceOption.link = new DiaNode("RejectedTruceDesc".Translate())
                {
                    options =
                    {
                        BackToRoot(faction, negotiator)
                    }
                };
            }
            return truceOption;
        }

        private static DiaOption OfferPeace(PoliticalAlliance alliance, Faction faction, Pawn negotiator, DominationConflict conflict)
        {
            int negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).Level;
            float openConflictFactor = conflict.GetWarEffort(alliance).WarWeariness;
            float chance = CalculateNegotiationChance(NEGOTIATE_PEACE_BASE_CHANCE, alliance, CFind.DominationTracker.PlayerAlliance, negotiatorSkill);
            DiaOption peaceOption = new DiaOption("OfferPeace".Translate(chance));
            if (Rand.Value < chance)
            {
                peaceOption.action = delegate
                {
                    CFind.DominationTracker.EndConflict(conflict, ConflictResult.Piece, CFind.DominationTracker.PlayerAlliance);
                };
                peaceOption.linkLateBind = FactionDialogMakerDiplomacy.ResetToRoot(faction, negotiator);
            }
            else
            {
                peaceOption.link = new DiaNode("RejectedPeaceDesc".Translate())
                {
                    options =
                    {
                        BackToRoot(faction, negotiator)
                    }
                };
            }
            return peaceOption;

        }

        private static DiaOption OfferSubjugation(PoliticalAlliance alliance, Faction faction, Pawn negotiator, DominationConflict conflict)
        {
            float chance = alliance.LeadingFaction.def.hidden == true ? 0f : 1f;
            DiaOption diaOption = new DiaOption("OfferSubjugation".Translate(chance * 100))
            {
                action = delegate
                {
                    CFind.DominationTracker.EndConflict(conflict, ConflictResult.Piece, alliance);
                },
                linkLateBind = FactionDialogMakerDiplomacy.ResetToRoot(faction, negotiator)
            };
            if (chance == 0f)
            {
                diaOption.Disable("CannotSubjugate".Translate());
            }
            return diaOption;
        }

        private static DiaOption DemandSubjugation(PoliticalAlliance alliance, Faction faction, Pawn negotiator, DominationConflict conflict)
        {
            int negotiatorSkill = negotiator.skills.GetSkill(SkillDefOf.Social).Level;
            float chance = 0f;
            PoliticalAlliance player = CFind.DominationTracker.PlayerAlliance;
            if (!conflict.GetWarEffort(alliance).isWeary)
            {
                DiaOption disabledOption = new DiaOption("DemandSubjugation".Translate(chance));
                disabledOption.Disable("AllianceNotWeary".Translate());
                return disabledOption;
            }
            chance = CalculateNegotiationChance(NEGOTIATE_SUBJUGATION_BASE_CHANCE, alliance, CFind.DominationTracker.PlayerAlliance, negotiatorSkill);
            var subjugateOption = new DiaOption("DemandSubjugation".Translate(chance * 100));
            if (Rand.Value < chance)
            {
                subjugateOption.link = new DiaNode("RejectedSubjugationDesc")
                {
                    options =
                        {
                            OKToRoot(faction, negotiator)

                    }

                };
            }
            else
            {
                subjugateOption.action = delegate
                {
                    CFind.DominationTracker.EndConflict(conflict, ConflictResult.Subjugation, CFind.DominationTracker.PlayerAlliance);
                };
                subjugateOption.linkLateBind = FactionDialogMakerDiplomacy.ResetToRoot(faction, negotiator);
            }
            return subjugateOption;
        }

        private static float CalculateNegotiationChance(float baseChance, PoliticalAlliance alliance, PoliticalAlliance player, int negotiatorSkill)
        {
            float powerBalance = alliance.CombinedPowerAgainst(player) / (player.CombinedPowerAgainst(alliance) + alliance.CombinedPowerAgainst(player));

            return Math.Min(1f, baseChance + powerBalance + negotiatorSkill / 50f);
        }

        private static DiaNode PlayerAllianceNPCMemberNode(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            var node = new DiaNode("PlayerAllianceNPCMember".Translate());
            node.options.Add(DemandTributeFrom(alliance, faction, negotiator));
            node.options.Add(ExpellFromAlliance(alliance, faction, negotiator));
            node.options.Add(SetTaxes(alliance, faction, negotiator));

            return node;
        }

        private static DiaOption SetTaxes(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            return new DiaOption("SetTaxes".Translate())
            {
                action = delegate
                {

                }
            };
        }

        private static DiaOption ExpellFromAlliance(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            return new DiaOption("ExpellFromAlliance".Translate())
            {
                action = delegate
                 {
                     alliance.RemoveFromAlliance(faction, false);
                     faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, true, "ExpelledFromAllianceNPC".Translate());
                 },
                linkLateBind = ResetToRoot(faction, negotiator)                 
            };
        }

        private static DiaOption DemandTributeFrom(PoliticalAlliance alliance, Faction faction, Pawn negotiator)
        {
            return new DiaOption("DemandTribute")
            {
                action = delegate
                {

                }
            };
        }
        
        private static DiaOption BackToRoot(Faction faction, Pawn negotiator)
        {
            return new DiaOption("Back".Translate())
            {
                linkLateBind = FactionDialogMakerDiplomacy.ResetToRoot(faction, negotiator)
            };
        }

        private static Func<DiaNode> ResetToRoot(Faction faction, Pawn negotiator)
        {
            return () => FactionDialogMaker.FactionDialogFor(negotiator, faction);
        }


        private static DiaOption OKToRoot(Faction faction, Pawn negotiator)
        {
            return new DiaOption("OK".Translate())
            {
                linkLateBind = FactionDialogMakerDiplomacy.ResetToRoot(faction, negotiator)
            };
        }
    }
}
