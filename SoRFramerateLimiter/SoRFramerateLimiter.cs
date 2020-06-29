using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace SoRFramerateLimiter
{
    public class Globals
    {
        public static GameObject targetFPSButton = null;

        public static int MaxFPS => 999;
        public static int FPSIndex { get; set; } = 1;
        public static readonly int[] FPSPresets = new int[] { 60, 75, 120, 144, 165, 168, 240 };

        public static void ChangeFPSIndex(int val)
        {
            FPSIndex += val;
            if (FPSIndex < 0) FPSIndex = FPSPresets.Length - 1;
            if (FPSIndex >= FPSPresets.Length) FPSIndex = 0;

            targetFPSButton.transform.Find("Text2").gameObject.GetComponent<UnityEngine.UI.Text>().text = FPSPresets[FPSIndex].ToString();
            Application.targetFrameRate = FPSPresets[FPSIndex];

            // Write changes to file.
            if (!Directory.Exists($"{Application.persistentDataPath}/ModConfig/"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/ModConfig/");
            }

            using(var sw = new StreamWriter($"{Application.persistentDataPath}/ModConfig/SoRFramerateLimiterConfig.txt"))
            {
                sw.Write(FPSIndex);
            }
        }

        public static void LoadAndApplyConfig()
        {
            try
            {
                using(var sr = new StreamReader($"{Application.persistentDataPath}/ModConfig/SoRFramerateLimiterConfig.txt"))
                {
                    FPSIndex = int.Parse(sr.ReadToEnd().Trim());
                }
            }
            catch
            {
                FPSIndex = 0;
            }

            Application.targetFrameRate = FPSPresets[FPSIndex];
        }

        public static string TargetFPSButtonName => "TargetFPSButton";
        public static string TargetFPSButtonText => "Target Framerate";
    }

    [BepInPlugin(ID, NAME, VERSION)]
    public class SoRFramerateLimiter : BaseUnityPlugin
    {
        const string ID = "com.0x6495ED.rogframeratelimiter";
        const string NAME = "Streets of Rogue Framerate Limiter";
        const string VERSION = "1.0";

        internal void Awake()
        {
            var harmony = new Harmony(ID);
            harmony.PatchAll();
            harmony.PatchAll(typeof(MenuGUIGamepadPatch));
        }
    }

    [HarmonyPatch(typeof(MenuGUI), "RealAwake")]
    public class MenuGUI_RealAwake
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            var mainMenu = GameObject.Find("MainGUI").transform.Find("MainMenu");
            var gfxMenuContent = mainMenu.transform.Find("SettingsGraphics/ScrollView/Content").gameObject;
            var monNumButton = mainMenu.transform.Find("SettingsGraphics/ScrollView/Content/MonitorNumButton").gameObject;

            var targetFPSButton = UnityEngine.Object.Instantiate(monNumButton);

            targetFPSButton.transform.SetParent(gfxMenuContent.transform);
            targetFPSButton.transform.SetSiblingIndex(6);
            targetFPSButton.name = Globals.TargetFPSButtonName;
            targetFPSButton.transform.localPosition = new Vector3(0.0f, -120.0f, -100.0f);

            targetFPSButton.GetComponent<MenuButtonHelper>().myText.text = Globals.TargetFPSButtonText;

            // Setup ordering, the "lazy way"
            var buttons = new string[] 
            { "BorderlessWindowButton", "ResetDefaultsButtonGraphics", "DoneButtonGraphics" };

            for(var i = 0; i < buttons.Length; i++)
            {
                var obj = gfxMenuContent.transform.Find(buttons[i]).gameObject;
                var offset = 80.0f * (i + 1);
                obj.transform.localPosition = new Vector3(0.0f, -120.0f - offset, -100.0f);
            }

            Globals.targetFPSButton = targetFPSButton;
            Globals.LoadAndApplyConfig();
        }
    }

    [HarmonyPatch(typeof(MenuGUI), "OpenSettingsGraphics")]
    public class MenuGUI_OpenSettingsGraphics
    {
        public static void Postfix(ref float ___curNumChoices, ref GameObject ___settingsGraphics, 
            ref GameObject ___settingsGraphicsContent, ref UnityEngine.UI.Scrollbar ___curScrollBar)
        {
            // Enable the scrollbar and make room for our new entry.
            ___curNumChoices = 9.0f;
            ___settingsGraphicsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, ___curNumChoices * 80f);
            ___curScrollBar = ___settingsGraphics.transform.Find("Scrollbar").GetComponent<UnityEngine.UI.Scrollbar>();
            ___curScrollBar.value = 1f;

            // Purge the Left/Right button onClick so they don't affect monitor settings.
            var decButton = Globals.targetFPSButton.transform.Find("Button").gameObject.GetComponent<UnityEngine.UI.Button>();
            var incButton = Globals.targetFPSButton.transform.Find("Button2").gameObject.GetComponent<UnityEngine.UI.Button>();

            incButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            decButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();

            decButton.onClick.AddListener(() => Globals.ChangeFPSIndex(-1));
            incButton.onClick.AddListener(() => Globals.ChangeFPSIndex(1));

            // Update the values of the text
            Globals.ChangeFPSIndex(0); // Does nothing, causes a refresh.
        }
    }


    public static class MenuGUIGamepadPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuGUI), "SelectUp")]
        public static bool SelectUp(ref UnityEngine.UI.Selectable ___curSelected, ref UnityEngine.UI.Selectable ___prevSelected) 
        {
            if (___curSelected.name == "BorderlessWindowButton")
            {
                ___prevSelected.GetComponent<MenuButtonHelper>().mouseEnteredSlot = false;
                ___curSelected = Globals.targetFPSButton.GetComponent<UnityEngine.UI.Selectable>();
                return false;
            }
            else if (___curSelected.name == "TargetFPSButton")
            {
                ___prevSelected.GetComponent<MenuButtonHelper>().mouseEnteredSlot = false;
                ___curSelected = GameObject.Find("MainGUI/MainMenu/SettingsGraphics/ScrollView/Content/VSyncButton").GetComponent<UnityEngine.UI.Selectable>();
                return false;
            }

            return true;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuGUI), "SelectDown")]
        public static bool SelectDownPrefix(ref UnityEngine.UI.Selectable ___curSelected, ref UnityEngine.UI.Selectable ___prevSelected) 
        {
            if (___curSelected.name == "VSyncButton")
            {
                ___prevSelected.GetComponent<MenuButtonHelper>().mouseEnteredSlot = false;
                ___curSelected = Globals.targetFPSButton.GetComponent<UnityEngine.UI.Selectable>();
                return false;
            }
            else if (___curSelected.name == "TargetFPSButton")
            {
                ___prevSelected.GetComponent<MenuButtonHelper>().mouseEnteredSlot = false;
                ___curSelected = GameObject.Find("MainGUI/MainMenu/SettingsGraphics/ScrollView/Content/BorderlessWindowButton").GetComponent<UnityEngine.UI.Selectable>();
                return false;
            }


            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuGUI), "SelectLeft")]
        public static bool SelectLeftPrefix(UnityEngine.UI.Selectable ___curSelected)
        {
            if (___curSelected.name != Globals.targetFPSButton.name)
            {
                return true;
            }

            Globals.ChangeFPSIndex(-1);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuGUI), "SelectRight")]
        public static bool SelectRightPrefix(UnityEngine.UI.Selectable ___curSelected) 
        {
            if (___curSelected.name != Globals.targetFPSButton.name)
            {
                return true;
            }

            Globals.ChangeFPSIndex(1);

            return false;
        }
    }

    // Required to patch tooltip on mouse hoover for button.
    [HarmonyPatch(typeof(MenuGUI), "MousedOverInstructionText")]
    public class MenuGUI_MousedOverInstructionText
    {
        public static bool Prefix(string type, ref UnityEngine.UI.Text ___instructionText)
        {
            if (type == Globals.TargetFPSButtonName)
            {
                ___instructionText.text = "Sets the targeted framerate of the game. If VSync is turned On, this does nothing.";
                return false;
            }
            return true;
        }
    }

    // Sets the text at localization lookup, fixes a bug of text not setting correctly the first time.
    [HarmonyPatch(typeof(MenuButtonHelper), "SetupText3")]
    public class MenuButtonHelper_SetupText3
    {
        public static void Postfix(UnityEngine.UI.Text ___myText)
        {
            if (___myText.text == $"E_{Globals.TargetFPSButtonName}")
            {
                ___myText.text = Globals.TargetFPSButtonText;
            }
        }
    }
}