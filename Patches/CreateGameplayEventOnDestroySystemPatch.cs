using Bloodcraft.Interfaces;
using Bloodcraft.Resources;
using Bloodcraft.Services;


using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace Bloodcraft.Patches;

[HarmonyPatch]
internal static class CreateGameplayEventOnDestroySystemPatch
{
    static readonly bool _professions = ConfigService.ProfessionSystem;
    static readonly bool _quests = ConfigService.QuestSystem;

    const int BASE_PROFESSION_XP = 100;
    const float SCT_DELAY = 0.75f;

    static readonly PrefabGUID _fishingTravelToTarget = PrefabGUIDs.AB_Fishing_Draw_TravelToTarget;
    static readonly PrefabGUID _fishingQuestGoal = PrefabGUIDs.FakeItem_AnyFish;

    [HarmonyPatch(typeof(CreateGameplayEventOnDestroySystem), nameof(CreateGameplayEventOnDestroySystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(CreateGameplayEventOnDestroySystem __instance)
    {
        if (!Core._initialized) return;
        else if (!_professions && !_quests) return;

        NativeArray<Entity> entities = __instance.__query_1297357609_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity entity in entities)
            {
                if (!entity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists() || !entity.TryGetComponent(out PrefabGUID prefabGUID)) continue;
                else if (prefabGUID.Equals(_fishingTravelToTarget)) // succesful fishing event
                {
                    Entity playerCharacter = entityOwner.Owner;
                    Entity userEntity = playerCharacter.GetUserEntity();

                    User user = userEntity.GetUser();
                    ulong steamId = user.PlatformId;

                    PrefabGUID prefabGuid = PrefabGUID.Empty;
                    Entity target = entity.GetBuffTarget();

                  

                    if (target.TryGetBuffer<DropTableBuffer>(out var buffer)
                        && !buffer.IsEmpty)
                    {
                        prefabGuid = buffer[0].DropTableGuid;
                    }

                    if (prefabGuid.IsEmpty()) continue;

                    IProfession handler = ProfessionFactory.GetProfession(prefabGuid);
                    if (handler != null)
                    {
                        Profession profession = handler.GetProfessionEnum();
                       
                    }
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}