using H3C_BPMT_E.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace H3C_BPMT_E.Views.Pages
{
    public partial class ConsoleChangePage : INavigableView<ConsoleChangeViewModel>
    {
        public ConsoleChangeViewModel ViewModel { get; }

        public ConsoleChangePage(ConsoleChangeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
