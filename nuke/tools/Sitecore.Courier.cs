public static partial class CourierTasks
{
    public static string GetToolPath() => Build.RootDirectory / "buildmodules/Sitecore.Courier.Runner/Sitecore.Courier.Runner.exe";
}

public partial class CourierSettings
{
    public static string GetToolPath() => CourierTasks.GetToolPath();
}
