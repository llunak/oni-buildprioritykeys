using HarmonyLib;

namespace BuildPriorityKeys
{
    // Handle the number keys the same way e.g. BrushTool does (interceptNumberKeysForPriority).
    // The MaterialSelectionPanel class is what shows the priorities when placing a building,
    // but that class does not have the methods that need to be patched (they are inherited),
    // so it's necessary to patch the base class and filter it.

    [HarmonyPatch(typeof(KScreen))]
    public class KScreen_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnKeyDown))]
        public static bool OnKeyDown(KScreen __instance, KButtonEvent e)
        {
            if(!(__instance is MaterialSelectionPanel))
                return true;
            Action action = e.GetAction();
            if (Action.Plan1 <= action && action <= Action.Plan10 && e.TryConsume(action))
            {
                int num = (int)(action - 36 + 1);
                if (num <= 9)
                {
                    (__instance as MaterialSelectionPanel).PriorityScreen
                        .SetScreenPriority(new PrioritySetting(PriorityScreen.PriorityClass.basic, num), play_sound: true);
                }
                else
                {
                    (__instance as MaterialSelectionPanel).PriorityScreen
                        .SetScreenPriority(new PrioritySetting(PriorityScreen.PriorityClass.topPriority, 1), play_sound: true);
                }
            }
            return !e.Consumed;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnKeyUp))]
        public static bool OnKeyUp(KScreen __instance, KButtonEvent e)
        {
            if(!(__instance is MaterialSelectionPanel))
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
            if(!(__instance is MaterialSelectionPanel))
                return true;
            __result = 3f;
            return false;
        }
    }
}
