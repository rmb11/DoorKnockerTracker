using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using SillowApp.Data;
using SillowApp.Models;
using SillowApp.ViewModels;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SillowApp.Views;

public partial class JobsPage : ContentPage
{
    private const double MobileBreakpoint = 800;
    private const double MenuWidth = 200;
    private bool _isDesktop = false;
    private bool _menuIsOpened = false;
    private bool _isJobBeingAdded = false;

    public JobsPage()
    {
        InitializeComponent();
        BindingContext = new JobsPageViewModel();
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
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(MenuWidth) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(NavMenuComponent, 0);
            Grid.SetColumn(JobContent, 1);

            NavMenuComponent.TranslationX = 0;
            NavMenuComponent.IsVisible = true;
            HamburgerButton.IsVisible = false;

            if (PageTitle != null)
                PageTitle.Margin = new Thickness(0, 0, 0, 0);
        }
        else
        {
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(NavMenuComponent, 0);
            Grid.SetColumn(JobContent, 0);

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
            await NavMenuComponent.TranslateTo(-MenuWidth, 0, 250, Easing.CubicIn);
        }
        else
        {
            await NavMenuComponent.TranslateTo(0, 0, 250, Easing.CubicOut);
        }

        _menuIsOpened = !_menuIsOpened;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        UpdateLayout(_isDesktop);

        await Database.InitAsync();

        if (BindingContext is JobsPageViewModel viewModel)
        {
            await viewModel.LoadJobsAsync();
        }

        LoadGoogleMap();
        RegisterWebViewBridge();

#if !WINDOWS
        await Task.Delay(1000); // Wait for map to render (needed especially on Android)
        await LoadSavedMarkers();
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        MapWebView.Source = null;

#if WINDOWS
        try
        {
            if (MapWebView.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.WebView2 platformView && platformView.CoreWebView2 != null)
            {
                platformView.CoreWebView2.WebMessageReceived -= OnWebMessageReceived;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebView cleanup failed: {ex.Message}");
        }
#endif
    }

    private void RegisterWebViewBridge()
    {
#if WINDOWS
        MapWebView.Navigated += async (s, e) =>
        {
            await LoadSavedMarkers();

            var platformView = MapWebView.Handler.PlatformView as Microsoft.UI.Xaml.Controls.WebView2;
            if (platformView != null)
            {
                if (platformView.CoreWebView2 != null)
                {
                    platformView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                }
                else
                {
                    platformView.CoreWebView2Initialized += (sender, args) =>
                    {
                        platformView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                    };
                }
            }
        };
#elif ANDROID
        MapWebView.Navigating += async (s, e) =>
        {
            if (e.Url.StartsWith("jsbridge:"))
            {
                e.Cancel = true;
                var json = Uri.UnescapeDataString(e.Url.Substring("jsbridge:".Length));
                await HandleIncomingJobJson(json);
            }
        };
#endif
    }

#if WINDOWS
    private async void OnWebMessageReceived(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
    {
        var json = args.TryGetWebMessageAsString();
        await HandleIncomingJobJson(json);
    }
#endif

    private async Task HandleIncomingJobJson(string json)
    {
        if (_isJobBeingAdded) return; 
        _isJobBeingAdded = true;

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var job = JsonSerializer.Deserialize<Job>(json, options);

            if (job != null)
            {
                var existingJobs = await Database.GetJobsAsync();
                bool exists = existingJobs.Any(j =>
                    j.JobAddress == job.JobAddress &&
                    Math.Abs((j.CreatedAt - DateTime.UtcNow).TotalSeconds) < 3);

                if (!exists)
                {
                    await Database.AddJobAsync(job);
                    await DisplayAlert("Saved", $"Job '{job.Title}' saved successfully!", "OK");

                    if (BindingContext is JobsPageViewModel viewModel)
                        await viewModel.LoadJobsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            _isJobBeingAdded = false;
        }
    }

    private async void OnAddJobClicked(object sender, EventArgs e)
    {
        try
        {
            var newJob = new Job
            {
                Title = "",
                Description = "",
                Status = "New",
                CustomerName = "",
                CustomerEmail = "",
                CustomerPhone = "",
                JobAddress = "",
                JobDateTime = DateTime.Now,
                CreatedAt = DateTime.UtcNow
            };

            var modalPage = new JobEditModalPage(newJob, isNewJob: true);
            await Navigation.PushModalAsync(modalPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not open job editor: {ex.Message}", "OK");
        }
    }

    private async Task LoadGoogleMap()
    {
        string htmlContent;
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("maps.html");
            using var reader = new StreamReader(stream);
            htmlContent = await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load maps.html: {ex.Message}");
            return;
        }

        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High));
            if (location != null)
            {
                htmlContent = htmlContent
                    .Replace("%%lat%%", location.Latitude.ToString("F6", CultureInfo.InvariantCulture))
                    .Replace("%%long%%", location.Longitude.ToString("F6", CultureInfo.InvariantCulture));
            }
        }
        catch (Exception geoEx)
        {
            Console.WriteLine($"Geolocation error: {geoEx.Message}");
        }

        MapWebView.Source = new HtmlWebViewSource { Html = htmlContent };
    }

    private async Task LoadSavedMarkers()
    {
        try
        {
            var jobsList = await Database.GetJobsAsync();

            if (jobsList == null || !jobsList.Any())
            {
                Console.WriteLine("No saved jobs to load as markers.");
                return;
            }

            var markersData = jobsList
                .Where(j => j.Latitude.HasValue && j.Longitude.HasValue)
                .ToList();

            if (!markersData.Any())
            {
                Console.WriteLine("No jobs with coordinates found to load as markers.");
                return;
            }

            string markersJson = JsonSerializer.Serialize(markersData);

            string escapedMarkersJson = markersJson.Replace("\"", "\\\"");

            string script = $"loadMarkers('{escapedMarkersJson}');";
            await MapWebView.EvaluateJavaScriptAsync(script);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading saved markers: {ex.Message}");
        }
    }

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Job selectedJob)
            return;

        if (BindingContext is JobsPageViewModel viewModel)
        {
            if (viewModel.JobSelectedCommand.CanExecute(selectedJob))
            {
                await viewModel.JobSelectedCommand.ExecuteAsync(selectedJob);
            }
        }

        // De-select the item
        ((CollectionView)sender).SelectedItem = null;
    }
}