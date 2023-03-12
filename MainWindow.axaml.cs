using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
    }
}