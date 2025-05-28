using H3C_BPMT_E.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace H3C_BPMT_E.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
