#nullable disable
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Menu.Shop;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem;
using System;
using UnityEngine;
using IntPtr = System.IntPtr;
using Exception = System.Exception;

namespace Megaboenk
{
    [BepInPlugin("fi.pippel.megabonk.modmenu", "Megabonk Mod Menu", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo("Megabonk Mod Menu loaded");

            ClassInjector.RegisterTypeInIl2Cpp<ModMenuRunner>();

            var go = new GameObject("ModMenuRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<ModMenuRunner>();
        }
    }

    public class ModMenuRunner : MonoBehaviour
    {
        private IntPtr lastInventoryPtr = IntPtr.Zero;

        private bool menuOpen = false;
        private Rect windowRect = new Rect(20, 20, 360, 260);

        private bool pickupEnabled = true;
        private float pickupValue = 1000f;

        private bool luckEnabled = true;
        private float luckValue = 10000f;

        public ModMenuRunner(IntPtr ptr) : base(ptr) { }

        private void Update()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    menuOpen = !menuOpen;
                }

                var gm = UnityEngine.Object.FindObjectOfType<GameManager>();
                if (gm == null) return;

                var inv = gm.GetPlayerInventory();
                if (inv == null || inv.statInventory == null) return;

                var currentPtr = Il2CppInterop.Runtime.IL2CPP.Il2CppObjectBaseToPtr(inv);
                if (currentPtr == IntPtr.Zero) return;

                if (currentPtr != lastInventoryPtr)
                {
                    ApplyEnabledStats(inv);
                    lastInventoryPtr = currentPtr;
                }
            }
            catch (Exception ex)
            {
                BepInEx.Logging.Logger.CreateLogSource("Megabonk Mod Menu")
                    .LogError("Update failed: " + ex);
            }
        }

        private void OnGUI()
        {
            if (!menuOpen) return;

            windowRect = GUI.Window(123456, windowRect, (GUI.WindowFunction)DrawWindow, "Megabonk Mod Menu");
        }

        private void DrawWindow(int id)
        {
            GUI.Label(new Rect(10, 25, 200, 20), "F7 = open / close");

            pickupEnabled = GUI.Toggle(new Rect(10, 55, 180, 20), pickupEnabled, "Enable Pickup Range");
            GUI.Label(new Rect(10, 80, 200, 20), "Pickup Range: " + pickupValue.ToString("0"));
            pickupValue = GUI.HorizontalSlider(new Rect(10, 105, 200, 20), pickupValue, 0f, 5000f);

            luckEnabled = GUI.Toggle(new Rect(10, 135, 180, 20), luckEnabled, "Enable Luck");
            GUI.Label(new Rect(10, 160, 200, 20), "Luck: " + luckValue.ToString("0"));
            luckValue = GUI.HorizontalSlider(new Rect(10, 185, 200, 20), luckValue, 0f, 50000f);

            if (GUI.Button(new Rect(10, 215, 120, 30), "Apply Now"))
            {
                TryApplyNow();
            }

            if (GUI.Button(new Rect(140, 215, 200, 30), "Preset 1000 / 10000"))
            {
                pickupEnabled = true;
                luckEnabled = true;
                pickupValue = 1000f;
                luckValue = 10000f;
                TryApplyNow();
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void TryApplyNow()
        {
            try
            {
                var gm = UnityEngine.Object.FindObjectOfType<GameManager>();
                if (gm == null) return;

                var inv = gm.GetPlayerInventory();
                if (inv == null || inv.statInventory == null) return;

                ApplyEnabledStats(inv);

                var currentPtr = Il2CppInterop.Runtime.IL2CPP.Il2CppObjectBaseToPtr(inv);
                if (currentPtr != IntPtr.Zero)
                {
                    lastInventoryPtr = currentPtr;
                }
            }
            catch (Exception ex)
            {
                BepInEx.Logging.Logger.CreateLogSource("Megabonk Mod Menu")
                    .LogError("TryApplyNow failed: " + ex);
            }
        }

        private void ApplyEnabledStats(PlayerInventory inv)
        {
            if (pickupEnabled)
            {
                ApplyStat(inv, EStat.PickupRange, pickupValue);
            }

            if (luckEnabled)
            {
                ApplyStat(inv, EStat.Luck, luckValue);
            }

            string pickupText = pickupEnabled ? pickupValue.ToString("0") : "OFF";
            string luckText = luckEnabled ? luckValue.ToString("0") : "OFF";
            BepInEx.Logging.Logger.CreateLogSource("Megabonk Mod Menu")
                .LogInfo("Applied stats -> Pickup: " + pickupText + ", Luck: " + luckText);
        }

        private void ApplyStat(PlayerInventory inv, EStat stat, float value)
        {
            var mod = new StatModifier();
            mod.stat = stat;
            mod.modifyType = EStatModifyType.Addition;
            mod.modification = value;

            inv.statInventory.ChangeStat(mod, true, 0f, false);
        }
    }
}