using System;
using System.Windows;
using WpfClient.Clients;

namespace WpfClient;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Guid _sessionId;
    private SecureClient _secureClient;

    public MainWindow()
    {
        _secureClient = new SecureClient();
        InitializeComponent();
    }

    protected override async void OnInitialized(EventArgs e)
    {
        await _secureClient.InitializeAsync();
        base.OnInitialized(e);
    }

    private async void OnSearchClick(object sender, RoutedEventArgs e)
    {
        var result = await _secureClient.GetFileAsync(this.fileName.Text);

        if (!result.IsSuccessful)
            this.textBlock.Text = "Failure: " + result.Error.ToString();
        else
            this.textBlock.Text = result.Result;

    }

    private async void OnReconnectClick(object sender, RoutedEventArgs e)
    {
        var result = await _secureClient.InitializeAsync();

        if (result.IsSuccessful)
        {
            MessageBox.Show("Critical fail, shutting down", "Error");
            Application.Current.Shutdown();
        }
    }
}
