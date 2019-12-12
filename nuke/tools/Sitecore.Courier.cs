
public static partial class CourierTasks
{
    public static string GetToolPath()
    {
        return Build.RootDirectory / "buildmodules/Sitecore.Courier.Runner/Sitecore.Courier.Runner.exe";
    }
}

public partial class CourierSettings
{
    public static string GetToolPath()
    {
        return CourierTasks.GetToolPath();
    }
}