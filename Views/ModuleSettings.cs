public static class ModuleSettings
{
    public static bool JobsEnabled = true;
    public static bool CRMEnabled = true;
    public static bool CalendarEnabled = true;
    public static bool TeamEnabled = true;

    public static event Action ModulesChanged;

    public static void SetModule(string module, bool enabled)
    {
        switch (module)
        {
            case "Jobs": JobsEnabled = enabled; break;
            case "CRM": CRMEnabled = enabled; break;
            case "Calendar": CalendarEnabled = enabled; break;
            case "Team": TeamEnabled = enabled; break;
        }
        ModulesChanged?.Invoke();
    }
}
