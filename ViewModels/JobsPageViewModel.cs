using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SillowApp.Data;
using SillowApp.Models;
using SillowApp.Views;

namespace SillowApp.ViewModels
{
    public partial class JobsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsMapView))]
        private bool _isListView = true;

        public bool IsMapView => !IsListView;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(JobsCount))]
        private ObservableCollection<Job> _jobsList;

        public int JobsCount => JobsList?.Count ?? 0;

        public JobsPageViewModel()
        {
            JobsList = new ObservableCollection<Job>();

            MessagingCenter.Subscribe<JobEditModalViewModel>(this, "JobUpdated", async (sender) =>
            {
                await LoadJobsAsync();
            });
        }

        [RelayCommand]
        private void ToggleToListView() => IsListView = true;

        [RelayCommand]
        private void ToggleToMapView() => IsListView = false;

        [RelayCommand]
        public async Task LoadJobsAsync()
        {
            var jobs = await Database.GetJobsAsync();

            var filteredJobs = jobs
                .Where(j => !string.Equals(j.Status, "Come Back Later", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(j.Status, "Do Not Return", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(j => j.CreatedAt);

            JobsList = new ObservableCollection<Job>(filteredJobs);
        }

        [RelayCommand]
        private async Task JobSelectedAsync(Job selectedJob)
        {
            if (selectedJob == null)
                return;

            var modalPage = new JobEditModalPage(selectedJob);

            await Application.Current.MainPage.Navigation.PushModalAsync(modalPage);
        }
    }
}