using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using OsuParsers.Replays;

namespace osrUI.Utils;

public class ReplayUtil
{
    public static void LoadReplayInfo(Replay? replay, Dictionary<TextBox, string?> allTextBoxes,
        Button[] buttons,
        CheckBox[] checkBoxes)
    {
        foreach (var i in allTextBoxes.Where(i => true))
        {
            i.Key.Text = i.Value;
            i.Key.IsEnabled = true;
        }

        foreach (var i in checkBoxes)
        {
            i.IsEnabled = true;
            if (i.Name == "IsPerfectComboCheckBox")
                i.IsChecked = replay?.PerfectCombo;
        }
        foreach (var i in buttons)
            i.IsEnabled = true;
    }
}