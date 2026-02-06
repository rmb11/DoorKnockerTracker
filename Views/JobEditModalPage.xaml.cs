using SillowApp.Models;
using SillowApp.ViewModels;

namespace SillowApp.Views
{
    public partial class JobEditModalPage : ContentPage
    {
        private readonly JobEditModalViewModel _viewModel;

        public JobEditModalPage(Job jobToEdit, bool isNewJob = false)
        {
            InitializeComponent();

            _viewModel = new JobEditModalViewModel(jobToEdit, isNewJob);

            BindingContext = _viewModel;
        }
    }
}
