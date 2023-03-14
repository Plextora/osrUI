using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using osrUI.Utils;
using OsuParsers.Beatmaps;
using OsuParsers.Decoders;
using OsuParsers.Enums;
using OsuParsers.Replays;

namespace osrUI;

public partial class MainWindow : Window
{
    private bool _isMouseDown;
    private PointerPoint _originalPoint;
    private readonly Label _statusLabel;
    private static Replay? _osuReplay;
    public static readonly Dictionary<TextBox, string?> TextBoxes = new();
    public static Button[] Buttons = Array.Empty<Button>();
    public static CheckBox[] CheckBoxes = Array.Empty<CheckBox>();
    public static Dictionary<TextBox, string>? JudgementTextBoxes;
    private static Dictionary<CheckBox, Mods>? _modCheckBoxList;
    private static readonly List<Mods>? _listOfModsApplied = new();

    public MainWindow()
    {
        InitializeComponent();

        _statusLabel = this.FindControl<Label>("StatusLabel");
    }

    #region Make window moveable

    private void MainPanel_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isMouseDown) return;

        var currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void MainPanel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

        _isMouseDown = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void MainPanel_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isMouseDown = false;
    }

    #endregion

    private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) => Close();

    private async void OpenReplayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        SetStatusLabel.Pending(_statusLabel, "Decoding replay file...");

        OpenFileDialog openFileDialog = new()
        {
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "osu! Replay files", Extensions = { "osr" } },
                new() { Name = "All files", Extensions = { "*" } }
            },
            Directory =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\osu!\Replays",
            Title = "Select a replay file"
        };

        var result = await openFileDialog.ShowAsync(new MainWindow());

        if (result != null)
            _osuReplay = ReplayDecoder.Decode(result.FirstOrDefault());
        else if (result == null)
        {
            SetStatusLabel.Default(_statusLabel);
            return;
        }

        OnReplayDecoded();
    }

    private async void OnReplayDecoded()
    {
        if (TextBoxes.Count != 0)
            TextBoxes.Clear();
        if (Buttons.Length != 0)
            Array.Clear(Buttons, 0, Buttons.Length);
        if (CheckBoxes.Length != 0)
            Array.Clear(CheckBoxes, 0, CheckBoxes.Length);

        TextBoxes.Add(ReplayUsernameTextBox, _osuReplay?.PlayerName);
        TextBoxes.Add(ReplayTimestampTextBox,
            _osuReplay?.ReplayTimestamp.ToLocalTime().ToString(CultureInfo.CurrentCulture));
        TextBoxes.Add(ScoreTextBox, _osuReplay?.ReplayScore.ToString());
        TextBoxes.Add(ComboTextBox, (_osuReplay?.Combo).ToString());
        TextBoxes.Add(_300sCountTextBox, _osuReplay?.Count300.ToString());
        TextBoxes.Add(_100sCountTextBox, _osuReplay?.Count100.ToString());
        TextBoxes.Add(_50sCountTextBox, _osuReplay?.Count50.ToString());
        TextBoxes.Add(MissCountTextBox, _osuReplay?.CountMiss.ToString());
        TextBoxes.Add(GekiCountTextBox, _osuReplay?.CountGeki.ToString());
        TextBoxes.Add(KatuCountTextBox, _osuReplay?.CountKatu.ToString());

        Buttons = new[] { OpenReplayButton, SaveReplayButton };
        CheckBoxes = new[] { IsPerfectComboCheckBox };

        JudgementTextBoxes = new Dictionary<TextBox, string>
        {
            { _300sCountTextBox, "number of 300s" },
            { _100sCountTextBox, "number of 100s" },
            { _50sCountTextBox, "number of 50s" },
            { MissCountTextBox, "number of misses" },
            { GekiCountTextBox, "number of Geki's" },
            { KatuCountTextBox, "number of Katu's" }
        };

        ReplayUtil.LoadReplayInfo(_osuReplay, TextBoxes, Buttons, CheckBoxes);

        SetStatusLabel.Completed(_statusLabel, "Loaded replay info!");
        await Task.Delay(2000);
        SetStatusLabel.Default(_statusLabel);
    }

    private async void SaveReplayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ReplayUtil.CheckFields(StatusLabel, ComboTextBox, ScoreTextBox, JudgementTextBoxes, ReplayTimestampTextBox))
        {
            if (_listOfModsApplied != null && _listOfModsApplied.Count != 0)
                _listOfModsApplied.Clear();

            SetStatusLabel.Pending(_statusLabel, "Saving replay file...");

            SaveFileDialog saveFileDialog = new()
            {
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "osu! Replay files", Extensions = { "osr" } },
                    new() { Name = "All files", Extensions = { "*" } }
                },
                Directory =
                    $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\osu!\Replays",
                Title = "Save as replay file"
            };

            var result = await saveFileDialog.ShowAsync(new MainWindow());

            if (result == null)
            {
                SetStatusLabel.Default(_statusLabel);
                return;
            }

            if (_osuReplay != null)
            {
                _modCheckBoxList = new Dictionary<CheckBox, Mods>
                {
                    { EzModCheckBox, Mods.Easy },
                    { NfModCheckBox, Mods.NoFail },
                    { HtModCheckBox, Mods.HalfTime },
                    { HrModCheckBox, Mods.HardRock },
                    { SdModCheckBox, Mods.SuddenDeath },
                    { DtModCheckBox, Mods.DoubleTime },
                    { HdModCheckBox, Mods.Hidden },
                    { FlModCheckBox, Mods.Flashlight }
                };

                foreach (var i in _modCheckBoxList)
                    if ((bool)i.Key.IsChecked)
                        _listOfModsApplied?.Add(i.Value);

                _osuReplay.Mods = Mods.None;
                if (_listOfModsApplied != null)
                    foreach (var i in _listOfModsApplied)
                        _osuReplay.Mods |= i;

                if ((bool)HrModCheckBox.IsChecked)
                    foreach (var i in _osuReplay.ReplayFrames)
                        i.Y = 384f - i.Y; // flip replay if hr is selected, thanks mrflash https://github.com/mrflashstudio/OsuParsers/issues/34#issuecomment-774695289

                _osuReplay.PlayerName = ReplayUsernameTextBox.Text;
                _osuReplay.Combo = Convert.ToUInt16(ComboTextBox.Text);
                _osuReplay.ReplayScore = Convert.ToInt32(ScoreTextBox.Text);
                if (IsPerfectComboCheckBox.IsChecked != null)
                    _osuReplay.PerfectCombo = (bool)IsPerfectComboCheckBox.IsChecked;
                _osuReplay.Count300 = Convert.ToUInt16(_300sCountTextBox.Text);
                _osuReplay.Count100 = Convert.ToUInt16(_100sCountTextBox.Text);
                _osuReplay.Count50 = Convert.ToUInt16(_50sCountTextBox.Text);
                _osuReplay.CountMiss = Convert.ToUInt16(MissCountTextBox.Text);
                _osuReplay.CountGeki = Convert.ToUInt16(GekiCountTextBox.Text);
                _osuReplay.CountKatu = Convert.ToUInt16(KatuCountTextBox.Text);
                _osuReplay.ReplayTimestamp = Convert.ToDateTime(ReplayTimestampTextBox.Text);

                _osuReplay.Save(result);
                SetStatusLabel.Completed(_statusLabel, "Saved edited replay!");
                await Task.Delay(2000);
                SetStatusLabel.Default(_statusLabel);
            }
        }
    }
}