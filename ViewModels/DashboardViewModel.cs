using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace SillowApp.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public const string BusinessName = "Sillow";
        public const string UserName = "SillowAppCrew";

        private string _currentDate = System.DateTime.Now.ToString("dddd, MMMM d, yyyy");
        private int _totalJobs;
        private int _completedJobs;
        private string _recentJobText = "No jobs yet.";
        private string _upcomingJobText = "No upcoming jobs.";

        public string CurrentDate
        {
            get => GetCurrentDateFormatted();
        }
        private string GetCurrentDateFormatted()
        { 
            return System.DateTime.Now.ToString("dddd, MMMM d, yyyy");
        }

        public void UpdateDateTime()
        {
            OnPropertyChanged(nameof(CurrentDate));
        }

        public int TotalJobs
        {
            get => _totalJobs;
            set { _totalJobs = value; OnPropertyChanged(); }
        }

        public int CompletedJobs
        {
            get => _completedJobs;
            set { _completedJobs = value; OnPropertyChanged(); }
        }

        public string RecentJobText
        {
            get => _recentJobText;
            set { _recentJobText = value; OnPropertyChanged(); }
        }

        public string UpcomingJobText
        {
            get => _upcomingJobText;
            set { _upcomingJobText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
