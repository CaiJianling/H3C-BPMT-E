using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace H3C_BPMT_E.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - H3C BPMT E (Batch Password Modification ToolEdition)";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "用户密码修改",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Password16 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Console 口密码修改",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Password20 },
                TargetPageType = typeof(Views.Pages.ConsoleChangePage)
            },
            new NavigationViewItem()
            {
                Content = "历史",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
                TargetPageType = typeof(Views.Pages.DataPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "设置",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
