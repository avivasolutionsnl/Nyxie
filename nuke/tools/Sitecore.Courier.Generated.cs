
using JetBrains.Annotations;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
///   <p>Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS.</p>
///   <p>For more details, visit the <a href="https://karma-runner.github.io/">official website</a>.</p>
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class CourierTasks
{
    /// <summary>
    ///   Path to the Courier executable.
    /// </summary>
    public static string CourierPath =>
        ToolPathResolver.TryGetEnvironmentExecutable("COURIER_EXE") ??
        GetToolPath();
    public static Action<OutputType, string> CourierLogger { get; set; } = ProcessTasks.DefaultLogger;
    /// <summary>
    ///   <p>Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS.</p>
    ///   <p>For more details, visit the <a href="https://karma-runner.github.io/">official website</a>.</p>
    /// </summary>
    public static IReadOnlyCollection<Output> Courier(string arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, Func<string, string> outputFilter = null)
    {
        var process = ProcessTasks.StartProcess(CourierPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, CourierLogger, outputFilter);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS.</p>
    ///   <p>For more details, visit the <a href="https://karma-runner.github.io/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>-f</c> via <see cref="CourierSettings.AddFiles"/></li>
    ///     <li><c>-o</c> via <see cref="CourierSettings.OutputPackage"/></li>
    ///     <li><c>-r</c> via <see cref="CourierSettings.RainbowFormat"/></li>
    ///     <li><c>-s</c> via <see cref="CourierSettings.SourceFolder"/></li>
    ///     <li><c>-t</c> via <see cref="CourierSettings.TargetFolder"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> Courier(CourierSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new CourierSettings();
        var process = ProcessTasks.StartProcess(toolSettings);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS.</p>
    ///   <p>For more details, visit the <a href="https://karma-runner.github.io/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>-f</c> via <see cref="CourierSettings.AddFiles"/></li>
    ///     <li><c>-o</c> via <see cref="CourierSettings.OutputPackage"/></li>
    ///     <li><c>-r</c> via <see cref="CourierSettings.RainbowFormat"/></li>
    ///     <li><c>-s</c> via <see cref="CourierSettings.SourceFolder"/></li>
    ///     <li><c>-t</c> via <see cref="CourierSettings.TargetFolder"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> Courier(Configure<CourierSettings> configurator)
    {
        return Courier(configurator(new CourierSettings()));
    }
    /// <summary>
    ///   <p>Sitecore Courier aims to fill the gap between the development and production environments when building websites with Sitecore CMS.</p>
    ///   <p>For more details, visit the <a href="https://karma-runner.github.io/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>-f</c> via <see cref="CourierSettings.AddFiles"/></li>
    ///     <li><c>-o</c> via <see cref="CourierSettings.OutputPackage"/></li>
    ///     <li><c>-r</c> via <see cref="CourierSettings.RainbowFormat"/></li>
    ///     <li><c>-s</c> via <see cref="CourierSettings.SourceFolder"/></li>
    ///     <li><c>-t</c> via <see cref="CourierSettings.TargetFolder"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(CourierSettings Settings, IReadOnlyCollection<Output> Output)> Courier(CombinatorialConfigure<CourierSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(Courier, CourierLogger, degreeOfParallelism, completeOnFailure);
    }
}
#region CourierSettings
/// <summary>
///   Used within <see cref="CourierTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class CourierSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Courier executable.
    /// </summary>
    public override string ToolPath => base.ToolPath ?? GetToolPath();
    public override Action<OutputType, string> CustomLogger => CourierTasks.CourierLogger;
    /// <summary>
    ///   Source folder (optional, only needed for Delta Packages)
    /// </summary>
    public virtual string SourceFolder { get; internal set; }
    /// <summary>
    ///   Target folder
    /// </summary>
    public virtual string TargetFolder { get; internal set; }
    /// <summary>
    ///   Output package (will be created)
    /// </summary>
    public virtual string OutputPackage { get; internal set; }
    /// <summary>
    ///   Add if you want to use Rainbow serialization format, otherwise it will treat .yml as files
    /// </summary>
    public virtual bool? RainbowFormat { get; internal set; }
    /// <summary>
    ///   Add if you want to include files to the package when using Rainbow serialization format
    /// </summary>
    public virtual bool? AddFiles { get; internal set; }
    protected override Arguments ConfigureArguments(Arguments arguments)
    {
        arguments
          .Add("-s {value}", SourceFolder)
          .Add("-t {value}", TargetFolder)
          .Add("-o {value}", OutputPackage)
          .Add("-r", RainbowFormat)
          .Add("-f", AddFiles);
        return base.ConfigureArguments(arguments);
    }
}
#endregion
#region CourierSettingsExtensions
/// <summary>
///   Used within <see cref="CourierTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class CourierSettingsExtensions
{
    #region SourceFolder
    /// <summary>
    ///   <p><em>Sets <see cref="CourierSettings.SourceFolder"/></em></p>
    ///   <p>Source folder (optional, only needed for Delta Packages)</p>
    /// </summary>
    [Pure]
    public static CourierSettings SetSourceFolder(this CourierSettings toolSettings, string sourceFolder)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.SourceFolder = sourceFolder;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="CourierSettings.SourceFolder"/></em></p>
    ///   <p>Source folder (optional, only needed for Delta Packages)</p>
    /// </summary>
    [Pure]
    public static CourierSettings ResetSourceFolder(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.SourceFolder = null;
        return toolSettings;
    }
    #endregion
    #region TargetFolder
    /// <summary>
    ///   <p><em>Sets <see cref="CourierSettings.TargetFolder"/></em></p>
    ///   <p>Target folder</p>
    /// </summary>
    [Pure]
    public static CourierSettings SetTargetFolder(this CourierSettings toolSettings, string targetFolder)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.TargetFolder = targetFolder;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="CourierSettings.TargetFolder"/></em></p>
    ///   <p>Target folder</p>
    /// </summary>
    [Pure]
    public static CourierSettings ResetTargetFolder(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.TargetFolder = null;
        return toolSettings;
    }
    #endregion
    #region OutputPackage
    /// <summary>
    ///   <p><em>Sets <see cref="CourierSettings.OutputPackage"/></em></p>
    ///   <p>Output package (will be created)</p>
    /// </summary>
    [Pure]
    public static CourierSettings SetOutputPackage(this CourierSettings toolSettings, string outputPackage)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputPackage = outputPackage;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="CourierSettings.OutputPackage"/></em></p>
    ///   <p>Output package (will be created)</p>
    /// </summary>
    [Pure]
    public static CourierSettings ResetOutputPackage(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.OutputPackage = null;
        return toolSettings;
    }
    #endregion
    #region RainbowFormat
    /// <summary>
    ///   <p><em>Sets <see cref="CourierSettings.RainbowFormat"/></em></p>
    ///   <p>Add if you want to use Rainbow serialization format, otherwise it will treat .yml as files</p>
    /// </summary>
    [Pure]
    public static CourierSettings SetRainbowFormat(this CourierSettings toolSettings, bool? rainbowFormat)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.RainbowFormat = rainbowFormat;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="CourierSettings.RainbowFormat"/></em></p>
    ///   <p>Add if you want to use Rainbow serialization format, otherwise it will treat .yml as files</p>
    /// </summary>
    [Pure]
    public static CourierSettings ResetRainbowFormat(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.RainbowFormat = null;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Enables <see cref="CourierSettings.RainbowFormat"/></em></p>
    ///   <p>Add if you want to use Rainbow serialization format, otherwise it will treat .yml as files</p>
    /// </summary>
    [Pure]
    public static CourierSettings EnableRainbowFormat(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.RainbowFormat = true;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Disables <see cref="CourierSettings.RainbowFormat"/></em></p>
    ///   <p>Add if you want to use Rainbow serialization format, otherwise it will treat .yml as files</p>
    /// </summary>
    [Pure]
    public static CourierSettings DisableRainbowFormat(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.RainbowFormat = false;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Toggles <see cref="CourierSettings.RainbowFormat"/></em></p>
    ///   <p>Add if you want to use Rainbow serialization format, otherwise it will treat .yml as files</p>
    /// </summary>
    [Pure]
    public static CourierSettings ToggleRainbowFormat(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.RainbowFormat = !toolSettings.RainbowFormat;
        return toolSettings;
    }
    #endregion
    #region AddFiles
    /// <summary>
    ///   <p><em>Sets <see cref="CourierSettings.AddFiles"/></em></p>
    ///   <p>Add if you want to include files to the package when using Rainbow serialization format</p>
    /// </summary>
    [Pure]
    public static CourierSettings SetAddFiles(this CourierSettings toolSettings, bool? addFiles)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AddFiles = addFiles;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="CourierSettings.AddFiles"/></em></p>
    ///   <p>Add if you want to include files to the package when using Rainbow serialization format</p>
    /// </summary>
    [Pure]
    public static CourierSettings ResetAddFiles(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AddFiles = null;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Enables <see cref="CourierSettings.AddFiles"/></em></p>
    ///   <p>Add if you want to include files to the package when using Rainbow serialization format</p>
    /// </summary>
    [Pure]
    public static CourierSettings EnableAddFiles(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AddFiles = true;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Disables <see cref="CourierSettings.AddFiles"/></em></p>
    ///   <p>Add if you want to include files to the package when using Rainbow serialization format</p>
    /// </summary>
    [Pure]
    public static CourierSettings DisableAddFiles(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AddFiles = false;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Toggles <see cref="CourierSettings.AddFiles"/></em></p>
    ///   <p>Add if you want to include files to the package when using Rainbow serialization format</p>
    /// </summary>
    [Pure]
    public static CourierSettings ToggleAddFiles(this CourierSettings toolSettings)
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AddFiles = !toolSettings.AddFiles;
        return toolSettings;
    }
    #endregion
}
#endregion
