using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using osrUI.Utils;
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

    private void OpenReplayButton_OnClick(object? sender, RoutedEventArgs e)
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

        var result = openFileDialog.ShowAsync(new MainWindow());

        if (result.Result != null)
        {
            _osuReplay = ReplayDecoder.Decode(result.Result.FirstOrDefault());
        }
        else if (result.Result == null)
        {
            SetStatusLabel.Default(_statusLabel); // this might not be necessary because of async stuff
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
        TextBoxes.Add(ComboTextBox, (_osuReplay?.Combo).ToString());
        TextBoxes.Add(_300sCountTextBox, _osuReplay?.Count300.ToString());
        TextBoxes.Add(_100sCountTextBox, _osuReplay?.Count100.ToString());
        TextBoxes.Add(_50sCountTextBox, _osuReplay?.Count50.ToString());
        TextBoxes.Add(MissCountTextBox, _osuReplay?.CountMiss.ToString());
        TextBoxes.Add(GekiCountTextBox, _osuReplay?.CountGeki.ToString());
        TextBoxes.Add(KatuCountTextBox, _osuReplay?.CountKatu.ToString());

        Buttons = new[] { OpenReplayButton, SaveReplayButton };
        CheckBoxes = new[] { IsPerfectComboCheckBox };

        ReplayUtil.LoadReplayInfo(_osuReplay, TextBoxes, Buttons, CheckBoxes);

        SetStatusLabel.Completed(_statusLabel, "Loaded replay info!");
        await Task.Delay(2000);
        SetStatusLabel.Default(_statusLabel);
    }

    private void SaveReplayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}