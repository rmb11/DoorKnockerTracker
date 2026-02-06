using Microsoft.Maui.Controls;
using SillowApp.Data;
using SillowApp.ViewModels;
using System;
using System.Threading.Tasks;

namespace SillowApp.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;

        private const double MobileBreakpoint = 800;
        private const double MenuWidth = 200;
        private const uint AnimationDuration = 250; 

        private bool _isDesktop = false;
        private bool _menuIsOpened = false;

        public DashboardPage()
        {
            InitializeComponent();
            _viewModel = new DashboardViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            bool newIsDesktop = width > MobileBreakpoint;

            if (newIsDesktop != _isDesktop)
            {
                _isDesktop = newIsDesktop;
                UpdateLayout(_isDesktop);
            }

            if (!_isDesktop && !_menuIsOpened)
            {
                NavMenuComponent.TranslationX = -MenuWidth;
            }
        }

        private void UpdateLayout(bool isDesktop)
        {
            MainGrid.ColumnDefinitions.Clear();

            if (isDesktop)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = MenuWidth });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                Grid.SetColumn(NavMenuComponent, 0);
                Grid.SetColumn(DashboardContent, 1);

                NavMenuComponent.TranslationX = 0;
                NavMenuComponent.IsVisible = true;
                HamburgerButton.IsVisible = false;

                if (PageTitle != null)
                    PageTitle.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                Grid.SetColumn(NavMenuComponent, 0);
                Grid.SetColumn(DashboardContent, 0);

                NavMenuComponent.IsVisible = true;
                NavMenuComponent.TranslationX = -MenuWidth;
                HamburgerButton.IsVisible = true;
                _menuIsOpened = false;

                if (PageTitle != null)
                    PageTitle.Margin = new Thickness(70, 0, 0, 0);
            }
        }


        private async void HamburgerButton_Clicked(object sender, EventArgs e)
        {
            if (_isDesktop) return;

            if (_menuIsOpened)
            {
                await DashboardContent.FadeTo(1.0, AnimationDuration / 2);
                await NavMenuComponent.TranslateTo(-MenuWidth, 0, AnimationDuration, Easing.CubicIn);
            }
            else
            {
                await NavMenuComponent.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut);
                await DashboardContent.FadeTo(0.7, AnimationDuration / 2);
            }

            _menuIsOpened = !_menuIsOpened;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.UpdateDateTime();

            await LoadStatsAsync();

            UpdateLayout(_isDesktop);

            if (DeviceInfo.Platform == DevicePlatform.WinUI && App.DashboardWindow != null)
            {
                App.DashboardWindow.MinimumWidth = 510;
                App.DashboardWindow.MinimumHeight = 850;
            }
        }

        private async Task LoadStatsAsync()
        {
            await Database.InitAsync();

            var total = await Database.GetBookedJobCountAsync();
            var completed = await Database.GetCompletedJobCountAsync();
            var returnVisits = await Database.GetReturnVisitsCountAsync();
            var recent = await Database.GetMostRecentJobAsync();
            var upcoming = await Database.GetUpcomingJobAsync();

            TotalJobsLabel.Text = total.ToString();
            CompletedJobsLabel.Text = completed.ToString();
            ReturnVisitsLabel.Text = returnVisits.ToString();
            RecentJobLabel.Text = recent != null
                ? $"{recent.Title} ({recent.Status ?? "No Status"})"
                : "No jobs yet.";
            UpcomingJobLabel.Text = upcoming != null
                ? $"{upcoming.Title} (Due {upcoming.CreatedAt:MMM d})"
                : "No upcoming jobs.";
        }

        private void JobButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new JobsPage());
            if (_menuIsOpened)
            {
                HamburgerButton_Clicked(sender, e);
            }
        }

        private void CRMButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CRMPage());
            if (_menuIsOpened)
            {
                HamburgerButton_Clicked(sender, e);
            }
        }

        private void ViewAllLabel_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new JobsPage());
        }
    }
}
