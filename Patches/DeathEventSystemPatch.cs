using Bloodcraft.Services;

using Bloodcraft.Systems.Legacies;

using Bloodcraft.Utilities;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using static Bloodcraft.Services.DataService.FamiliarPersistence;

using static Bloodcraft.Utilities.Familiars;

namespace Bloodcraft.Patches;

[HarmonyPatch]
internal static class DeathEventListenerSystemPatch
{
    static readonly bool _leveling = ConfigService.LevelingSystem;
    static readonly bool _expertise = ConfigService.ExpertiseSystem;
    static readonly bool _familiars = ConfigService.FamiliarSystem;
    static readonly bool _legacies = ConfigService.LegacySystem;
    static readonly bool _professions = ConfigService.ProfessionSystem;
    static readonly bool _allowMinions = ConfigService.FamiliarSystem && ConfigService.AllowMinions;
    public class DeathEventArgs : EventArgs
    {
        public Entity Source { get; set; }
        public Entity Target { get; set; }
        public HashSet<Entity> DeathParticipants { get; set; }
        public float ScrollingTextDelay { get; set; }
    }
    public static event EventHandler<DeathEventArgs> OnDeathEventHandler;
    static void RaiseDeathEvent(DeathEventArgs deathEvent)
    {
        OnDeathEventHandler?.Invoke(null, deathEvent);
    }

    [HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
    [HarmonyPostfix]
    static unsafe void OnUpdatePostfix(DeathEventListenerSystem __instance)
    {
        if (!Core._initialized) return;

        using NativeAccessor<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArrayAccessor<DeathEvent>();

        ComponentLookup<Movement> movementLookup = __instance.GetComponentLookup<Movement>(true);
        ComponentLookup<BlockFeedBuff> blockFeedBuffLookup = __instance.GetComponentLookup<BlockFeedBuff>(true);
        ComponentLookup<Trader> traderLookup = __instance.GetComponentLookup<Trader>(true);
        ComponentLookup<UnitLevel> unitLevelLookup = __instance.GetComponentLookup<UnitLevel>(true);
        ComponentLookup<Minion> minionLookup = __instance.GetComponentLookup<Minion>(true);
        ComponentLookup<VBloodConsumeSource> vBloodConsumeSourceLookup = __instance.GetComponentLookup<VBloodConsumeSource>(true);

        try
        {
            for (int i = 0; i < deathEvents.Length; i++)
            {
                DeathEvent deathEvent = deathEvents[i];

               
                 if (movementLookup.HasComponent(deathEvent.Died))
                {
                    Entity deathSource = ValidateSource(deathEvent.Killer);

                    bool isFeedKill = deathEvent.StatChangeReason.Equals(StatChangeReason.HandleGameplayEventsBase_11);
                    bool isMinion = minionLookup.HasComponent(deathEvent.Died);

                    if (deathSource.Exists())
                    {
                        DeathEventArgs deathArgs = new()
                        {
                            Source = deathSource,
                            Target = deathEvent.Died,
                            DeathParticipants = Progression.GetDeathParticipants(deathSource),
                            ScrollingTextDelay = 0f
                        };

                        if (isMinion)
                        {
                            if (_allowMinions)
                            {
                              
                            }

                            continue;
                        }

                        RaiseDeathEvent(deathArgs);

                        if (_legacies && isFeedKill)
                        {
                            // deathArgs.RefreshStats = false;
                            BloodSystem.ProcessLegacy(deathArgs);
                        }
                    }
                }
                else if (_professions && deathEvent.Killer.IsPlayer())
                {

                }
            }
        }
        finally
        {
            // deathEvents.Dispose();
        }
    }
    static Entity ValidateSource(Entity source)
    {
        Entity deathSource = Entity.Null;
        if (source.IsPlayer()) return source; // players

        if (!source.TryGetComponent(out EntityOwner entityOwner)) return deathSource;
        else if (entityOwner.Owner.TryGetPlayer(out Entity player)) deathSource = player; // player familiars and player summons
        else if (entityOwner.Owner.TryGetFollowedPlayer(out Entity followedPlayer)) deathSource = followedPlayer; // familiar summons

        return deathSource;
    }
   




        }
    
