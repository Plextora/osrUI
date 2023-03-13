using System;
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

    public static bool CheckFields(Label statusLabel,
        TextBox? comboTextBox,
        TextBox? scoreTextBox,
        Dictionary<TextBox, string>? judgementTextBoxes,
        TextBox? replayTimestampTextBox)
    {
        if (CheckCombo(statusLabel, comboTextBox) && CheckScore(statusLabel, scoreTextBox) &&
            CheckJudgments(statusLabel, judgementTextBoxes) && CheckTimestamp(statusLabel, replayTimestampTextBox))
            return true;
        return false;
    }

    private static bool CheckCombo(Label statusLabel, TextBox? comboTextBox)
    {
        if (comboTextBox != null && comboTextBox.Text.Any(char.IsLetter))
        {
            SetStatusLabel.Error(statusLabel, "The combo textbox cannot contain letters");
            return false;
        }

        if (comboTextBox != null && comboTextBox.Text.Contains(','))
        {
            SetStatusLabel.Error(statusLabel, "The combo textbox cannot have commas!");
            return false;
        }

        try
        {
            var int32 = Convert.ToInt32(comboTextBox?.Text);
        }
        catch (FormatException)
        {
            SetStatusLabel.Error(statusLabel,
                "The combo textbox is in an invalid format. Numbers only");
            return false;
        } // extra extra fail safe, I could just use this instead of all the other stuff above.

        try
        {
            var uInt16 = Convert.ToUInt16(comboTextBox?.Text);
        }
        catch (OverflowException)
        {
            SetStatusLabel.Error(statusLabel, "Combo number must be higher than 0 but lower than 65,535!");
            return false;
        }

        return true;
    }

    private static bool CheckScore(Label statusLabel, TextBox? scoreTextBox)
    {
        if (scoreTextBox != null && scoreTextBox.Text.Any(char.IsLetter))
        {
            SetStatusLabel.Error(statusLabel, "The score textbox cannot contain letters");
            return false;
        }

        if (scoreTextBox != null && scoreTextBox.Text.Contains(','))
        {
            SetStatusLabel.Error(statusLabel, "The score textbox cannot have commas!");
            return false;
        }

        try
        {
            var int32 = Convert.ToInt32(scoreTextBox?.Text);
        }
        catch (OverflowException)
        {
            SetStatusLabel.Error(statusLabel,
                "Score number must be higher than -2,147,483,648 but lower than 2,147,483,647!");
            return false;
        }

        try
        {
            var int32 = Convert.ToInt32(scoreTextBox?.Text);
        }
        catch (FormatException)
        {
            SetStatusLabel.Error(statusLabel,
                "The score textbox is in an invalid format. Numbers only");
            return false;
        } // extra extra fail safe, I could just use this instead of all the other stuff above.

        return true;
    }

    private static bool CheckJudgments(Label statusLabel, Dictionary<TextBox, string>? judgmentTextBoxes)
    {
        foreach (var i in judgmentTextBoxes)
        {
            if (i.Key.Text.Any(char.IsLetter))
            {
                SetStatusLabel.Error(statusLabel, $"The {i.Value} textbox cannot contain letters!");
                return false;
            }

            if (i.Key.Text.Contains(','))
            {
                SetStatusLabel.Error(statusLabel, $"The {i.Value} textbox cannot contain commas!");
                return false;
            }

            try
            {
                var uInt16 = Convert.ToUInt16(i.Key.Text);
            }
            catch (OverflowException)
            {
                SetStatusLabel.Error(statusLabel, $"The {i.Value} must be higher than 0 but lower than 65,535!");
                return false;
            }

            try
            {
                var uInt16 = Convert.ToUInt16(i.Key.Text);
            }
            catch (FormatException)
            {
                SetStatusLabel.Error(statusLabel,
                    $"The {i.Value} is in an invalid format. Numbers only");
                return false;
            } // extra extra fail safe, I could just use this instead of all the other stuff above.
        }

        return true;
    }

    private static bool CheckTimestamp(Label statusLabel, TextBox? replayTimestampTextBox)
    {
        try
        {
            var dateTime = Convert.ToDateTime(replayTimestampTextBox?.Text);
        }
        catch
        {
            SetStatusLabel.Error(statusLabel, "Invalid replay timestamp format!");
            return false;
        }

        return true;
    }
}