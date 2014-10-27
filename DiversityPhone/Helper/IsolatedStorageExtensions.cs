namespace DiversityPhone
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Threading.Tasks;

    public static class IsolatedStorageExtensions
    {
        /// <summary>
        /// Collects the Paths of Directories and Files inside a given Directory relative to it.
        /// </summary>
        /// <param name="Iso">The Isolated Storage to Use</param>
        /// <param name="directory">The Directory to crawl</param>
        /// <param name="relativeSubDirectories">relative Paths to the subdirectories in the given directory, ordered top - down</param>
        /// <param name="relativeSubFiles">relative Paths to the files in the given directory and its children, ordered top - down</param>
        private static void CollectSubdirectoriesAndFilesBreadthFirst(IsolatedStorageFile Iso, string directory, out IList<string> relativeSubDirectories, out IList<string> relativeSubFiles)
        {
            var relativeDirs = new List<string>();
            var relativeFiles = new List<string>();
            var toDo = new Queue<string>();

            toDo.Enqueue(string.Empty);

            while (toDo.Count > 0)
            {
                var relativeSubDir = toDo.Dequeue();
                var absoluteSubDir = Path.Combine(directory, relativeSubDir);
                var queryString = string.Format("{0}\\*", absoluteSubDir);

                foreach (var file in Iso.GetFileNames(queryString))
                {
                    relativeFiles.Add(Path.Combine(relativeSubDir, file));
                }

                foreach (var dir in Iso.GetDirectoryNames(queryString))
                {
                    var relativeSubSubdir = Path.Combine(relativeSubDir, dir);
                    toDo.Enqueue(relativeSubSubdir);
                    relativeDirs.Add(relativeSubSubdir);
                }
            }

            relativeSubDirectories = relativeDirs;
            relativeSubFiles = relativeFiles;
        }

        public static Task CopyDirectoryAsync(this IsolatedStorageFile Iso, string SourceDirectory, string TargetDirectory, IProgress<double> FractionProgress, bool OverWrite = false)
        {
            Contract.Requires(Iso != null);
            Contract.Requires(FractionProgress != null);
            Contract.Requires(Iso.DirectoryExists(SourceDirectory), "Source Directory does not exist");

            return Task.Factory.StartNew(() =>
            {
                FractionProgress.Report(0.0);

                IList<string> relativeFilePaths;
                IList<string> relativeDirPaths;
                CollectSubdirectoriesAndFilesBreadthFirst(Iso, SourceDirectory, out relativeDirPaths, out relativeFilePaths);

                var totalElementCount =
                    relativeDirPaths.Count + //SubDirectories
                    1 + //TargetDir
                    relativeFilePaths.Count; // Files

                var reporter = new PercentageReporter<double>(FractionProgress, p => p / 100.0, totalElementCount);

                var absoluteDirs = from relativeDir in relativeDirPaths
                                   select Path.Combine(TargetDirectory, relativeDir);

                foreach (var dir in Enumerable.Repeat(TargetDirectory, 1).Concat(absoluteDirs))
                {
                    if (!Iso.DirectoryExists(dir))
                    {
                        Iso.CreateDirectory(dir);
                    }
                    reporter.Completed++;
                }

                foreach (var relativeFile in relativeFilePaths)
                {
                    var sourceFile = Path.Combine(SourceDirectory, relativeFile);
                    var targetFile = Path.Combine(TargetDirectory, relativeFile);

                    try
                    {
                        Iso.CopyFile(sourceFile, targetFile, OverWrite);
                    }
                    catch (IsolatedStorageException)
                    {
                        // Ignore
                    }
                    reporter.Completed++;
                }
            });
        }

        public static Task DeleteDirectoryRecursiveAsync(this IsolatedStorageFile Iso, string DirectoryPath)
        {
            Contract.Requires(Iso != null);

            return Task.Factory.StartNew(() =>
            {
                if (Iso.DirectoryExists(DirectoryPath))
                {
                    IList<string> filePathsTopDown;
                    IList<string> dirPathsTopDown;
                    CollectSubdirectoriesAndFilesBreadthFirst(Iso, DirectoryPath, out dirPathsTopDown, out filePathsTopDown);

                    foreach (var file in filePathsTopDown)
                    {
                        var absolutePath = Path.Combine(DirectoryPath, file);
                        Iso.DeleteFile(absolutePath);
                    }

                    foreach (var dir in dirPathsTopDown.Reverse())
                    {
                        var absolutePath = Path.Combine(DirectoryPath, dir);
                        Iso.DeleteDirectory(absolutePath);
                    }

                    Iso.DeleteDirectory(DirectoryPath);
                }
            });
        }
    }
}