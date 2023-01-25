//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Class to help code UI (mostly for navigation)
/// </summary>
public static class UIHelper
{

    public static void SelectOnUp(Selectable on, Selectable to)
    {
        Navigation n = on.navigation;
        n.selectOnUp = to;
        on.navigation = n;
    }
    public static void SelectOnDown(Selectable on, Selectable to)
    {
        Navigation n = on.navigation;
        n.selectOnDown = to;
        on.navigation = n;
    }
    public static void SelectOnLeft(Selectable on, Selectable to)
    {
        Navigation n = on.navigation;
        n.selectOnLeft = to;
        on.navigation = n;
    }
    public static void SelectOnRight(Selectable on, Selectable to)
    {
        Navigation n = on.navigation;
        n.selectOnRight = to;
        on.navigation = n;
    }
    public static bool NavEquals(Navigation o, Navigation t)
    {
        int count = 0;

        count = CheckSelect(o.selectOnUp, t.selectOnUp) ? count += 1 : count;
        count = CheckSelect(o.selectOnDown, t.selectOnDown) ? count += 1 : count;
        count = CheckSelect(o.selectOnLeft, t.selectOnLeft) ? count += 1 : count;
        count = CheckSelect(o.selectOnRight, t.selectOnRight) ? count += 1 : count;

        return count == 4 ? true : false;
    }
    static bool CheckSelect(Selectable one, Selectable two) => one == two ? true : false;
}
