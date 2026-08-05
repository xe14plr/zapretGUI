using System.Windows;
using System.Windows.Input;
using ZapretGUI.App.Controls;
using ZapretGUI.App.Services;
using ZapretGUI.App.Views;

namespace ZapretGUI.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AnimatedBackground _background;

    public MainWindow()
    {
        InitializeComponent();
        _background = new AnimatedBackground(BackgroundCanvas);

        Loaded += (_, _) =>
        {
            AppServices.Modal.Attach(OverlayLayer, Scrim, ModalPanel, ModalContent);
            _background.Start();
        };
        Closed += (_, _) => _background.Stop();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseModal_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Close();

    private void OpenSettings_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Show(new SettingsPage());

    private void OpenLists_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Show(new ListsPage());

    private void OpenDiagnostics_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Show(new DiagnosticsPage());

    private void OpenUpdates_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Show(new UpdatesPage());

    private void OpenAbout_Click(object sender, RoutedEventArgs e) => AppServices.Modal.Show(new AboutPage());
}
