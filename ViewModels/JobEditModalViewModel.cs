using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SillowApp.Data;
using SillowApp.Models;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace SillowApp.ViewModels
{
    [QueryProperty(nameof(JobToEdit), "JobToEdit")]
    public partial class JobEditModalViewModel : ObservableObject
    {
        private readonly bool _isNewJob;

        [ObservableProperty]
        private Job _jobToEdit;

        [ObservableProperty]
        private DateTime _jobDate;

        [ObservableProperty]
        private TimeSpan _jobTime;

        public JobEditModalViewModel(Job jobToEdit, bool isNewJob = false)
        {
            JobToEdit = jobToEdit;
            _isNewJob = isNewJob;

            if (JobToEdit?.JobDateTime != null)
            {
                JobDate = JobToEdit.JobDateTime.Value.Date;
                JobTime = JobToEdit.JobDateTime.Value.TimeOfDay;
            }
            else
            {
                JobDate = DateTime.Now;
                JobTime = DateTime.Now.TimeOfDay;
            }
        }

        partial void OnJobToEditChanged(Job value)
        {
            if (value?.JobDateTime != null)
            {
                JobDate = value.JobDateTime.Value.Date;
                JobTime = value.JobDateTime.Value.TimeOfDay;
            }
            else
            {
                JobDate = DateTime.Now;
                JobTime = DateTime.Now.TimeOfDay;
            }
        }

        [RelayCommand]
        private async Task SaveJobAsync()
        {
            if (JobToEdit is null) return;

            JobToEdit.JobDateTime = JobDate.Date + JobTime;

            if (_isNewJob)
            {
                JobToEdit.CreatedAt = DateTime.UtcNow;
                await Database.AddJobAsync(JobToEdit);
                await Application.Current.MainPage.DisplayAlert("Success", "Job added successfully!", "OK");
            }
            else
            {
                await Database.UpdateJobAsync(JobToEdit);
                await Application.Current.MainPage.DisplayAlert("Success", "Changes saved successfully!", "OK");
            }

            MessagingCenter.Send(this, "JobUpdated");
            await CloseModalAsync();
        }

        [RelayCommand]
        private async Task CompleteJobAsync()
        {
            if (JobToEdit is null) return;

            JobToEdit.Status = "Completed";
            await Database.UpdateJobAsync(JobToEdit);

            MessagingCenter.Send(this, "JobUpdated");

            await CloseModalAsync();
        }

        [RelayCommand] 
        private async Task DeleteJobAsync()
        {
            if (JobToEdit is null) return;

            bool confirmed = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete the job at {JobToEdit.JobAddress}?",
                "Yes, Delete", "No");

            if (confirmed)
            {
                await Database.DeleteJobAsync(JobToEdit.Id);

                MessagingCenter.Send(this, "JobUpdated");

                await CloseModalAsync();
            }
        }

        [RelayCommand]
        private async Task CloseModalAsync()
        { 
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}