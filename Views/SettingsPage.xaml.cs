using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace SillowApp.Views
{
    public partial class SettingsPage : ContentPage
    {
        private Dictionary<string, bool> tempModules = new();

        private const double MobileBreakpoint = 800;
        private const double MenuWidth = 200;
        private bool _isDesktop = false;
        private bool _menuIsOpened = false;

        public SettingsPage()
        {
            InitializeComponent();

            tempModules["Jobs"] = App.Modules["Jobs"];
            tempModules["CRM"] = App.Modules["CRM"];
            tempModules["Team"] = App.Modules["Team"];
            tempModules["Calendar"] = App.Modules["Calendar"];

            JobsToggle.IsToggled = tempModules["Jobs"];
            CRMtoggle.IsToggled = tempModules["CRM"];
            TeamsToggle.IsToggled = tempModules["Team"];
            CalendarToggle.IsToggled = tempModules["Calendar"];

            JobsToggle.Toggled += JobsToggle_Toggled;
            CRMtoggle.Toggled += CRMtoggle_Toggled;
            TeamsToggle.Toggled += TeamsToggle_Toggled;
            CalendarToggle.Toggled += CalendarToggle_Toggled;
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
                Grid.SetColumn(SettingsContent, 1);

                NavMenuComponent.TranslationX = 0;
                NavMenuComponent.IsVisible = true;
                HamburgerButton.IsVisible = false;
            }
            else
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Grid.SetColumn(NavMenuComponent, 0);
                Grid.SetColumn(SettingsContent, 0);

                NavMenuComponent.IsVisible = true;
                NavMenuComponent.TranslationX = -MenuWidth;
                HamburgerButton.IsVisible = true;
                _menuIsOpened = false;
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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (App.SettingsWindow != null)
            {
                App.SettingsWindow.MinimumWidth = 510;
                App.SettingsWindow.MinimumHeight = 850;
            }

            if (MainGrid != null)
            {
                UpdateLayout(_isDesktop);
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            foreach (var kvp in tempModules)
            {
                App.SetModule(kvp.Key, kvp.Value);
            }

            await Navigation.PopAsync();
        }

        private void JobsToggle_Toggled(object sender, ToggledEventArgs e)
        {
            tempModules["Jobs"] = e.Value;
        }

        private void CRMtoggle_Toggled(object sender, ToggledEventArgs e)
        {
            tempModules["CRM"] = e.Value;
        }

        private void TeamsToggle_Toggled(object sender, ToggledEventArgs e)
        {
            tempModules["Team"] = e.Value;
        }

        private void CalendarToggle_Toggled(object sender, ToggledEventArgs e)
        {
            tempModules["Calendar"] = e.Value;
        }
    }
}

