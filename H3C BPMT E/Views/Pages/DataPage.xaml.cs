using H3C_BPMT_E.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace H3C_BPMT_E.Views.Pages
{
    public partial class DataPage : INavigableView<DataViewModel>
    {
        public DataViewModel ViewModel { get; }

        public DataPage(DataViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
