using System;
using System.Collections.Generic;
using TMPro;

public sealed class InventoryTextManager
{
    public static void SetInventoryText(ref Queue<Action> queue, TextMeshProUGUI inventoryItemStat, int value)
    {
        queue.Enqueue(() => inventoryItemStat.SetText(string.Format("+{0}", value)));
    }

    public static void SetInventoryTextChest(ref Queue<Action> queue, TextMeshProUGUI inventoryItemStat, int value)
    {
        queue.Enqueue(() => inventoryItemStat.SetText(string.Format("+{0}%", value)));
    }

    public static void SetInventoryTextGloves(ref Queue<Action> queue, TextMeshProUGUI inventoryItemStat, double value)
    {
        queue.Enqueue(() => inventoryItemStat.SetText(string.Format("+{0:F2}%", value / 100.0)));
    }

    public static void SetInventoryTextBoots(ref Queue<Action> queue, TextMeshProUGUI inventoryItemStat, double value)
    {
        queue.Enqueue(() => inventoryItemStat.SetText(string.Format("+{0:F1}", value / 10.0)));
    }
}
