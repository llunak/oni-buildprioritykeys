using HarmonyLib;
using System.Collections.Generic;

namespace BuildPriorityKeys
{
    // Handle the number keys the same way e.g. BrushTool does (interceptNumberKeysForPriority).
    // The MaterialSelectionPanel class is what shows the priorities when placing a building,
    // but that class does not have the methods that need to be patched (they are inherited),
    // so it's necessary to patch the base class and filter it.

    [HarmonyPatch(typeof(KScreen))]
    public class KScreen_Patch
    {
        public static HashSet< MaterialSelectionPanel > interceptNumberKeysForPriority
            = new HashSet< MaterialSelectionPanel >();

        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnKeyDown))]
        public static bool OnKeyDown(KScreen __instance, KButtonEvent e)
        {
            MaterialSelectionPanel panel = __instance as MaterialSelectionPanel;
            if( panel == null || !interceptNumberKeysForPriority.Contains( panel ))
                return true;
            Action action = e.GetAction();
            if (Action.Plan1 <= action && action <= Action.Plan10 && e.TryConsume(action))
            {
                int num = (int)(action - 36 + 1);
                if (num <= 9)
                {
                    panel.PriorityScreen.SetScreenPriority(
                        new PrioritySetting(PriorityScreen.PriorityClass.basic, num), play_sound: true);
                }
                else
                {
                    panel.PriorityScreen.SetScreenPriority(
                        new PrioritySetting(PriorityScreen.PriorityClass.topPriority, 1), play_sound: true);
                }
            }
            return !e.Consumed;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnKeyUp))]
        public static bool OnKeyUp(KScreen __instance, KButtonEvent e)
        {
            MaterialSelectionPanel panel = __instance as MaterialSelectionPanel;
            if( panel == null || !interceptNumberKeysForPriority.Contains( panel ))
                return true;
            Action action = e.GetAction();
            if (Action.Plan1 <= action && action <= Action.Plan10)
                e.TryConsume(action);
            return !e.Consumed;
        }

        // A catch is that KScreenManager processes key event handling of all active KScreen
        // instances in the order of GetSortKey(), higher first, and PlanScreen (which I think
        // is the build menu handling) has higher priority and hijacks the keys, so it's
        // necessary to provide a higher priority for MaterialSelectionPanel to go first
        // if it's active.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GetSortKey))]
        public static bool GetSortKey(KScreen __instance, ref float __result)
        {
            MaterialSelectionPanel panel = __instance as MaterialSelectionPanel;
            if( panel == null )
                return true;
            __result = 3f;
            return false;
        }
    }

    [HarmonyPatch(typeof(MaterialSelectionPanel))]
    public class MaterialSelectionPanel_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnPrefabInit))]
        public static void OnPrefabInit(MaterialSelectionPanel __instance)
        {
            KScreen_Patch.interceptNumberKeysForPriority.Add( __instance );
        }
    }

}
