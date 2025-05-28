using H3C_BPMT_E.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace H3C_BPMT_E.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
