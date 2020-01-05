using CodeGenerator = Nuke.CodeGeneration.CodeGenerator;

partial class Build : NukeBuild
{
    Target GenerateNukeTools => _ => _
        .Executes(() =>
        {
            AbsolutePath buildDirectory = RootDirectory / "nuke";
            CodeGenerator.GenerateCode(buildDirectory / "tools");
        });
}
