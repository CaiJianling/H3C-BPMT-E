using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using System.Reflection;
using System.Security.Policy;

namespace H3C_BPMT_E.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersionWithoutUrl = String.Empty;

        [ObservableProperty]
        private string _url = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersionWithoutUrl = $"H3C BPMT E - {GetAssemblyVersion()}";
            AppVersionWithoutUrl += $"\r\n\r\n© 2025 宁波大学附属人民医院 All rights reserved.";
            AppVersionWithoutUrl += $"\r\n\r\nAuthor: 蔡健灵 ";
            Url = "https://github.com/CaiJianling/H3C-BPMT-E";

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }
        }
    }
}