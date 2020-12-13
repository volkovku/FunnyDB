using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    readonly string Version = "0.1.3";
    readonly string Authors = "Kirill Volkov (drDebugIt)";
    readonly string Description = "FunnyDB - a simple and lightweight query builder and object mapper for .Net";
    readonly string KeyWords = "sql ado-net db database dsl";

    readonly BuildConfig[] Projects =
    {
        new BuildConfig("FunnyDB", pack: true),
        new BuildConfig("FunnyDB.Postgres", pack: true),
        new BuildConfig("FunnyDB.Test", test: true)
    };

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            foreach (var project in Projects.Where(_ => _.Restore))
            {
                var target = project.Path(Solution);
                WriteHeader(target);
                NuGetRestore(_ => _.SetTargetPath(target));
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            foreach (var project in Projects.Where(_ => _.Compile))
            {
                var target = project.Path(Solution);
                WriteHeader(target);
                DotNetBuild(_ => _
                    .SetProjectFile(target)
                    .SetNoRestore(true)
                    .SetAuthors(Authors)
                    .SetVersion(Version)
                );
            }
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (var project in Projects.Where(_ => _.Test))
            {
                var target = project.Path(Solution);
                WriteHeader(target);
                DotNetTest(_ => _
                    .SetProjectFile(target)
                    .SetNoRestore(true)
                    .SetNoBuild(true));
            }
        });
    
    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            foreach (var project in Projects.Where(_ => _.Pack))
            {
                var target = project.Path(Solution);
                WriteHeader(target);

                DotNetPack(_ => _
                    .SetProject(target)
                    .SetConfiguration("Release")
                    .SetAuthors(Authors)
                    .SetDescription(Description)
                    .SetPackageTags(KeyWords)
                    .SetPackageLicenseUrl(@"https://github.com/volkovku/FunnyDB/blob/master/license")
                    .SetRepositoryUrl(@"https://github.com/volkovku/FunnyDB")
                    .SetVersion(Version)
                );

                var packagePath =
                    Solution.Directory / project.Name / "bin" / "Release" /
                    $"{project.Name}.{Version}.nupkg";

                DotNetNuGetPush(_ => _
                    .SetTargetPath(packagePath)
                    .SetApiKey(GetNuGetApiKey())
                    .SetSource("https://api.nuget.org/v3/index.json")
                );
            }
        });

    static void WriteHeader(string title)
    {
        Console.WriteLine("".PadLeft(title.Length + 7, '-'));
        Console.WriteLine("-- " + title);
        Console.WriteLine("".PadLeft(title.Length, '-'));
    }

    static string GetNuGetApiKey()
    {
        return Environment.GetEnvironmentVariable("nuget_api_key");
    }

    class BuildConfig
    {
        public BuildConfig(
            string name,
            bool restore = true,
            bool compile = true,
            bool test = false,
            bool pack = false)
        {
            Name = name;
            Restore = restore;
            Compile = compile;
            Test = test;
            Pack = pack;
        }

        public readonly string Name;
        public readonly bool Restore;
        public readonly bool Compile;
        public readonly bool Test;
        public readonly bool Pack;

        public string Path(Solution solution)
        {
            return solution.Directory / Name / $"{Name}.csproj";
        }
    }
}