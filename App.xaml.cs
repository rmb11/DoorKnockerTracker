using SillowApp.Views;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace SillowApp
{
    public partial class App : Application
    {
        public static Window DashboardWindow;
        public static Window JobsWindow;
        public static Window CRMWindow;
        public static Window CalendarWindow;
        public static Window TeamsWindow;
        public static Window SettingsWindow;

        public static Dictionary<string, bool> Modules = new()
        {
            { "Dashboard", true },
            { "Jobs", true },
            { "CRM", true },
            { "Calendar", true },
            { "Team", true },    
            { "Settings", true },
            { "Invoicing", true },
            { "Expenses", true },
            { "Analytics", true }
        };

        public static event Action ModulesChanged;

        public static void SetModule(string moduleName, bool isEnabled)
        {
            Modules[moduleName] = isEnabled;
            ModulesChanged?.Invoke();
        }

        public static Components.NavMenu? NavMenu { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            DashboardWindow = new Window(new NavigationPage(new DashboardPage()));
            return DashboardWindow;
        }
    }
}
