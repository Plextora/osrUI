using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace osrUI;

public partial class MainWindow : Window
{
    private bool _isMouseDown;
    private PointerPoint _originalPoint;

    public MainWindow()
    {
        InitializeComponent();
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
}