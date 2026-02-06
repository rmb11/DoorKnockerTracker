using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace SillowApp.Views;

public partial class CRMPage : ContentPage
{
    private const double MobileBreakpoint = 800;
    private const double MenuWidth = 200;
    private bool _isDesktop = false;
    private bool _menuIsOpened = false;

    public CRMPage()
    {
        InitializeComponent();
        statusPicker.SelectedIndex = 0;
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
            Grid.SetColumn(CrmContent, 1);

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
            Grid.SetColumn(CrmContent, 0);

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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        UpdateLayout(_isDesktop);
    }
    private void OverlayButton_Clicked(object sender, EventArgs e)
    {
        Overlay.IsVisible = false;
    }
}