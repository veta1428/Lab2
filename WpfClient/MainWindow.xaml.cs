using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
}
