namespace LoadOrderIPatch {
    extern alias Injections;
    using LoadOrderIPatch.Patches;
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    public static class FileUtil {
        static void CacheWSFilesImpl() {
            try {
                Log.Called();
                var timer = Stopwatch.StartNew();
                var wsPath = Entry.GamePaths.WorkshopModsPath;
                var res1 = Directory.GetFiles(wsPath, "*.dll", searchOption: SearchOption.AllDirectories)
                    .AsParallel()
                    .Select(path => {
                        if (File.Exists(path)) {
                            using (var fs = File.OpenRead(path)) { }
                        }
                        return path;
                    });
                var res2 = Directory.GetFiles(wsPath, "*.crp", searchOption: SearchOption.AllDirectories)
                    .AsParallel()
                    .Select(path => {
                        if (File.Exists(path) && !Packages.IsPathExcluded(path)) {
                            using (var fs = File.OpenRead(path)) { }
                        }
                        return path;
                    });
                var res = res1.Concat(res2).ToList();
                Log.Info($"caching access to {res.Count} files took {timer.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }


        class FileQueue {
            public string[] files;
            public int pointer;

            public FileQueue(string[] files) {
                this.files = files;
                pointer = 0;
            }

            public bool Dequeue(out string file) {
                int index = pointer++; // atomic
                if (index >= files.Length) {
                    file = null;
                    return false;
                } else {
                    file = files[index];
                    return true;
                }
            }
            public bool Finished => pointer >= files.Length;
        }

        static void CacheWSFilesImpl2() {
            try {
                Log.Called();
                var timer = Stopwatch.StartNew();

                var wsPath = Entry.GamePaths.WorkshopModsPath;
                var files1 = Directory.GetFiles(wsPath, "*.dll", searchOption: SearchOption.AllDirectories)
                    .Where(path => !path.Contains(Path.PathSeparator + "_"));
                var files2 = Directory.GetFiles(wsPath, "*.crp", searchOption: SearchOption.AllDirectories)
                    .Where(path => !Packages.IsPathExcluded(path));
                var files = files1.Concat(files2).ToArray();

                var fileQueue = new FileQueue(files);
                int nThreads = Math.Min(100, files.Length / 5);
                Thread[] threads = new Thread[nThreads];
                for(int i = 0; i < nThreads; ++i) {
                    threads[i] = new Thread(CacheFilesThread);
                    threads[i].Start(fileQueue);
                }

                while (!fileQueue.Finished)
                    Thread.Sleep(10);
                Log.Info($"caching access to {files.Length} files took {timer.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                Log.Exception(ex);
            }

            static void CacheFilesThread(object arg) {
                try {
                    var fileQueue = arg as FileQueue ?? throw new ArgumentException($"{arg} is not FileQueue");
                    while (fileQueue.Dequeue(out string file)) { 
                        using (var fs = File.OpenRead(file)) { }
                    }
                } catch (Exception ex) {
                    Log.Exception(ex);
                }
            }
        }

        /// <summary>open and close files to cache improve the speed of first time load.</summary>
        /// precondition: all dependent dlls are loaded
        public static void CacheWSFiles() {
            Log.Called();
            new Thread(CacheWSFilesImpl2).Start();
        }
    }
}
