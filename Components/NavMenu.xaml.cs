using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SillowApp.Components
{
    public partial class NavMenu : ContentView
    {
        private bool _isNavVisible = true;
        private Frame _selectedItem;

        private readonly Dictionary<string, Func<Page>> _pageMap;
        private readonly Dictionary<string, Frame> _buttonMap;
        private string _selectedPageName;

        public NavMenu()
        {
            InitializeComponent();

            RestoreSelection();
            UpdateButtonVisibility();
            App.ModulesChanged += UpdateButtonVisibility;

            _pageMap = new Dictionary<string, Func<Page>>
            {
                { "Dashboard", () => new Views.DashboardPage() },
                { "Jobs", () => new Views.JobsPage() },
                { "CRM", () => new Views.CRMPage() },
                { "Calendar", () => new Views.CalendarPage() },
                { "Team", () => new Views.TeamPage() },
                { "Settings", () => new Views.SettingsPage() }
            };

            // Map page names to their Frames
            _buttonMap = new Dictionary<string, Frame>
            {
                { "Dashboard", DashboardItem },
                { "Jobs", JobsItem },
                { "CRM", CRMItem },
                { "Calendar", CalendarItem },
                { "Team", TeamItem },
                { "Settings", SettingsItem }
            };
        }

        public void ToggleNavBar(object sender, EventArgs e)
        {
            _isNavVisible = !_isNavVisible;
            NavBar.IsVisible = _isNavVisible;
        }

        private async void OnNavTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.GestureRecognizers[0] is TapGestureRecognizer tap)
            {
                string pageName = tap.CommandParameter?.ToString();
                if (string.IsNullOrEmpty(pageName)) return;

                if (_pageMap.TryGetValue(pageName, out var pageFunc))
                {
                    Page targetPage = pageFunc.Invoke();
                    await Application.Current.MainPage.Navigation.PushAsync(targetPage);
                    UpdateSelectedButton(pageName);
                }
            }
        }

        private void UpdateSelectedButton(string pageName)
        {
            if (_selectedItem != null)
            {
                _selectedItem.BackgroundColor = Colors.Transparent;
                foreach (var lbl in (_selectedItem.Content as HorizontalStackLayout).Children.OfType<Label>())
                    lbl.TextColor = Color.FromArgb("#b6bfca");
            }

            if (_buttonMap.TryGetValue(pageName, out var newSelected))
            {
                _selectedItem = newSelected;
                _selectedItem.BackgroundColor = Color.FromArgb("#2e2f30");
                foreach (var lbl in (_selectedItem.Content as HorizontalStackLayout).Children.OfType<Label>())
                    lbl.TextColor = Color.FromArgb("#dba721");

                _selectedPageName = pageName;
            }
        }

        public void RestoreSelection()
        {
            if (!string.IsNullOrEmpty(_selectedPageName))
                UpdateSelectedButton(_selectedPageName);
        }

        public void HighlightCurrentPage(Page currentPage)
        {
            foreach (var kvp in _pageMap)
            {
                if (kvp.Value().GetType() == currentPage.GetType())
                {
                    UpdateSelectedButton(kvp.Key);
                    break;
                }
            }
        }
        private void UpdateButtonVisibility()
        {
            // Show/hide buttons based on App.Modules dictionary
            JobsItem.IsVisible = App.Modules.ContainsKey("Jobs") && App.Modules["Jobs"];
            CRMItem.IsVisible = App.Modules.ContainsKey("CRM") && App.Modules["CRM"];
            CalendarItem.IsVisible = App.Modules.ContainsKey("Calendar") && App.Modules["Calendar"];
            TeamItem.IsVisible = App.Modules.ContainsKey("Team") && App.Modules["Team"];
        }
    }
}
