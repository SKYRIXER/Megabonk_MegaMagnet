using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Menu.Shop;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using System;
using UnityEngine;

namespace Megaboenk
{
    [BepInPlugin("fi.pippel.megabonk.pickup", "Pickup Range Mod", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo("Pickup Range Mod loaded");

            ClassInjector.RegisterTypeInIl2Cpp<PickupRangeRunner>();

            var go = new GameObject("PickupRangeRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<PickupRangeRunner>();
        }
    }

    public class PickupRangeRunner : MonoBehaviour
    {
        private IntPtr lastInventoryPtr = IntPtr.Zero;

        public PickupRangeRunner(IntPtr ptr) : base(ptr) { }

        private void Update()
        {
            try
            {
                var gm = UnityEngine.Object.FindObjectOfType<GameManager>();
                if (gm == null) return;

                var inv = gm.GetPlayerInventory();
                if (inv == null || inv.statInventory == null) return;

                var currentPtr = Il2CppInterop.Runtime.IL2CPP.Il2CppObjectBaseToPtr(inv);
                if (currentPtr == IntPtr.Zero) return;

                // Same inventory, no need to apply the mod again
                if (currentPtr == lastInventoryPtr) return;

                var mod = new StatModifier();
                mod.stat = EStat.PickupRange;
                mod.modifyType = EStatModifyType.Addition;
                mod.modification = 1000f;

                inv.statInventory.ChangeStat(mod, true, 0f, false);

                BepInEx.Logging.Logger.CreateLogSource("Pickup Range Mod")
                    .LogInfo("Pickup range set to 1000 for new run");

                lastInventoryPtr = currentPtr;
            }
            catch (Exception ex)
            {
                BepInEx.Logging.Logger.CreateLogSource("Pickup Range Mod")
                    .LogError($"Failed applying pickup range:" + ex);
            }
        }
    }
}