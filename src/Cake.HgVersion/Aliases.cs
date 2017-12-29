using System;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using HgVersion;
using HgVersion.VCS;
using HgVersion.VersionAssemblyInfoResources;
using VCSVersion;
using VCSVersion.Output;
using Mercurial;

namespace Cake.HgVersion
{
    /// <summary>
    /// Contains functionality for versioning using Mercurial history.
    /// <code>
    ///     #addin Cake.HgVersion
    /// </code>
    /// </summary>
    [CakeAliasCategory("HgVersion")]
    public static class Aliases
    {
        static Aliases()
        {
            Logger.SetLoggers(
                s => Console.WriteLine(s),
                s => Console.WriteLine(s),
                s => Console.WriteLine(s),
                s => Console.WriteLine(s));
        }
        
        /// <summary>
        /// Return information about current project version.
        /// </summary>
        /// <param name="context">Cake context</param>
        /// <param name="repositoryPath">Path to repository</param>
        /// <returns>Current project version</returns>
        [CakeMethodAlias]
        public static VersionVariables GetVersion(this ICakeContext context, DirectoryPath repositoryPath)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (repositoryPath == null) throw new ArgumentNullException(nameof(repositoryPath));

            using (context.AddLoggers())
            {
                var path = repositoryPath.MakeAbsolute(context.Environment);
                var repository = new Repository(path.FullPath);
                var versionContext = new HgVersionContext((HgRepository)repository);

                var finder = new VersionFinder();
                var version = finder.FindVersion(versionContext);
                var variables = version.ToVersionVariables(versionContext);
            
                return variables;   
            }
        }

        /// <summary>
        /// Update assembly info file or generate if threre is no such file.
        /// </summary>
        /// <param name="context">Cake context</param>
        /// <param name="variables">Version information variables</param>
        /// <param name="workingDirectory">Directory where assembly info file is located</param>
        [CakeMethodAlias]
        public static void UpdateAssemblyInfo(this ICakeContext context, VersionVariables variables, DirectoryPath workingDirectory)
        {
            var assemblyInfoFile = "SolutionInfo.cs";
            var fileSystem = new VCSVersion.Helpers.FileSystem();
            
            using (context.AddLoggers())
            using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFile, workingDirectory.FullPath, variables, fileSystem, true))
            {
                assemblyInfoFileUpdater.Update();
                assemblyInfoFileUpdater.CommitChanges();
            }
        }
    }
}