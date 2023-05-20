namespace KianCommons.Plugins {
    using System;
    using ColossalFramework.Plugins;
    using ICities;
    using System.Reflection;
    using ColossalFramework;
    using static ColossalFramework.Plugins.PluginManager;
    using ColossalFramework.PlatformServices;
    using System.Linq;
    using System.Collections.Generic;

    internal static class DelegateUtil {
        /// <typeparam name="TDelegate">delegate type</typeparam>
        /// <returns>Type[] representing arguments of the delegate.</returns>
        internal static Type[] GetParameterTypes<TDelegate>()
            where TDelegate : Delegate =>
            typeof(TDelegate)
            .GetMethod("Invoke")
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        /// <summary>
        /// Gets directly declared method based on a delegate that has
        /// the same name as the target method
        /// </summary>
        /// <param name="type">the class/type where the method is declared</param>
        /// <param name="name">the name of the method</param>
        internal static MethodInfo GetMethod<TDelegate>(this Type type, string name) where TDelegate : Delegate {
            var ret = type.GetMethod(
                name,
                types: GetParameterTypes<TDelegate>());
            if(ret == null) 
                Log.Warning($"could not find method {type.Name}.{name}");
            return ret;
        }

        internal static TDelegate CreateDelegate<TDelegate>(Type type, string name = null) where TDelegate : Delegate {
            var method = type.GetMethod<TDelegate>(name ?? typeof(TDelegate).Name);
            if (method == null) return null;
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), method);
        }
    }


    internal static class PluginExtensions {
        public static IUserMod GetUserModInstance(this PluginInfo plugin) => plugin?.userModInstance as IUserMod;

        public static string GetModName(this PluginInfo plugin) => GetUserModInstance(plugin)?.Name;

        public static ulong GetWorkshopID(this PluginInfo plugin) => plugin.publishedFileID.AsUInt64;

        /// <summary>
        /// shortcut for plugin?.isEnabled ?? false
        /// </summary>
        public static bool IsActive(this PluginInfo plugin) => plugin?.isEnabled ?? false;

        public static Assembly GetMainAssembly(this PluginInfo plugin) => plugin?.userModInstance?.GetType()?.Assembly;

        public static bool IsLocal(this PluginInfo plugin) =>
            plugin != null && (plugin.GetWorkshopID() == 0 || plugin.publishedFileID == PublishedFileId.invalid);

        public static PluginInfo GetPlugin(this Assembly assembly) => PluginUtil.GetPlugin(assembly);
    }

    internal static class PluginUtil {
        static PluginManager man => PluginManager.instance;

        public static PluginInfo GetCurrentAssemblyPlugin() => GetPlugin(Assembly.GetExecutingAssembly());

        public static void LogPlugins(bool detailed=false) {
            string PluginToString(PluginInfo p) {
                string enabled = p.isEnabled ? "*" : " ";
                string id = p.IsLocal() ? "(local)" : p.GetWorkshopID().ToString();
                id.PadRight(12);
                if(!detailed)
                    return $"\t{enabled} {id} {p.GetModName()}";
                return $"\t{enabled} " +
                    $"{id} " +
                    $"mod-name:{(p?.GetModName()).ToSTR()} " +
                    $"asm-name:{(p?.GetMainAssembly()?.Name()).ToSTR()} " +
                    $"user-mod-type:{(p?.userModInstance?.GetType()?.Name).ToSTR()}";
            }
            static int Comparison(PluginInfo a, PluginInfo b) {
                if (b == null) return 1;
                if (a == null) return -1;
                return b.isEnabled.CompareTo(a.isEnabled);
            }
            var plugins = man.GetPluginsInfo().ToList();
            plugins.Sort(Comparison); // enabled first
            var m = plugins.Select(p => PluginToString(p)).JoinLines();
            Log.Info("Installed mods are:\n" + m, true);
        }


        public static void ReportIncomaptibleMods(IEnumerable<PluginInfo> plugins) {
            // TODO complete:
        }

        public static PluginInfo GetCSUR() => GetPlugin("CSUR ToolBox", 1959342332ul);
        public static PluginInfo GetAdaptiveRoads() => GetPlugin("AdaptiveNetworks");
        public static PluginInfo GetHideCrossings() => GetPlugin("HideCrosswalks", searchOptions: AssemblyEquals);
        public static PluginInfo GetHideUnconnectedTracks() => GetPlugin("HideUnconnectedTracks", searchOptions: AssemblyEquals);
        public static PluginInfo GetDirectConnectRoads() => GetPlugin("DirectConnectRoads", searchOptions: AssemblyEquals);
        public static PluginInfo GetTrafficManager() => GetPlugin("TrafficManager", searchOptions: AssemblyEquals);
        public static PluginInfo GetNetworkDetective() => GetPlugin("NetworkDetective", searchOptions: AssemblyEquals);
        public static PluginInfo GetNetworkSkins() => GetPlugin("NetworkSkins", searchOptions: AssemblyEquals);
        public static PluginInfo GetNodeController() => GetPlugin("Node Controller");
        public static PluginInfo GetPedestrianBridge() => GetPlugin("Pedestrian Bridge");
        public static PluginInfo GetIMT() => GetPlugin("Intersection Marking", new[] { 2140418403ul, 2159934925ul});
        public static PluginInfo GetRAB() => GetPlugin("Roundabout Builder");
        
        public static PluginInfo GetSkyveMod() => GetPlugin("SkyveMod", searchOptions: AssemblyEquals);

        [Obsolete]
        internal static bool CSUREnabled;
        [Obsolete]
        static bool IsCSUR(PluginInfo current) =>
            current.name.Contains("CSUR ToolBox") || 1959342332 == (uint)current.publishedFileID.AsUInt64;
        [Obsolete]
        public static void Init() {
            CSUREnabled = false;
            foreach (PluginInfo current in man.GetPluginsInfo()) {
                if (!current.isEnabled) continue;
                if (IsCSUR(current)) {
                    CSUREnabled = true;
                    Log.Debug(current.name + "detected");
                    return;
                }
            }
        }

        public static PluginInfo GetPlugin(this IUserMod userMod) {
            foreach (PluginInfo current in man.GetPluginsInfo()) {
                if (userMod == current.userModInstance)
                    return current;
            }
            return null;
        }

        public static PluginInfo GetPlugin<UserModT> () where UserModT: IUserMod {
            foreach(PluginInfo plugin in man.GetPluginsInfo()) {
                if(plugin.userModInstance is UserModT)
                    return plugin;
            }
            return null;
        }

        public static PluginInfo GetPlugin(Assembly assembly = null) {
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();
            foreach (PluginInfo current in man.GetPluginsInfo()) {
                if (current.ContainsAssembly(assembly))
                    return current;
            }
            return null;
        }

        [Flags]
        public enum SearchOptionT {
            None=0,

            Contains = 1<<0,

            StartsWidth = 1<<1,

            [Obsolete("always active")]
            Equals = 1<<2,

            AllModes = Contains | StartsWidth,

            /// <summary></summary>
            CaseInsensetive = 1 << 3,

            /// <summary></summary>
            IgnoreWhiteSpace = 1 << 4,

            AllOptions = CaseInsensetive | IgnoreWhiteSpace,

            /// <summary>search for IUserMod.Name</summary>
            UserModName = 1<<5,

            /// <summary>search for the type of user mod instance excluding name space</summary>
            UserModType = 1<<6,

            /// <summary>search for the root name space of user mod type</summary>
            RootNameSpace = 1<<7,

            /// <summary>search for the PluginInfo.name</summary>
            PluginName = 1<<8,

            /// <summary>search for the name of the main assembly</summary>
            AssemblyName =  1<<9,

            AllTargets = UserModName | UserModType | RootNameSpace | PluginName | AssemblyName,
        }


        public const SearchOptionT DefaultsearchOptions =
            SearchOptionT.Contains | SearchOptionT.AllOptions | SearchOptionT.UserModName;

        public const SearchOptionT AssemblyEquals =
            SearchOptionT.AllOptions | SearchOptionT.AssemblyName;

        public static PluginInfo GetPlugin(
            string searchName, ulong searchId, SearchOptionT searchOptions = DefaultsearchOptions) {
            return GetPlugin(searchName, new[] { searchId }, searchOptions);
        }

        public static PluginInfo GetPlugin(
            string searchName, ulong[] searchIds = null, SearchOptionT searchOptions = DefaultsearchOptions) {
            foreach (PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (current == null) continue;

                bool match = Matches(current, searchIds);

                IUserMod userModInstance = current.userModInstance as IUserMod;
                if (userModInstance == null) continue;

                if (searchOptions.IsFlagSet(SearchOptionT.UserModName))
                    match = match || Match(userModInstance.Name, searchName, searchOptions);

                Type userModType = userModInstance.GetType();
                if (searchOptions.IsFlagSet(SearchOptionT.UserModType))
                    match = match || Match(userModType.Name, searchName, searchOptions);

                if (searchOptions.IsFlagSet(SearchOptionT.RootNameSpace)) {
                    string ns = userModType.Namespace;
                    string rootNameSpace = ns.Split('.')[0];
                    match = match || Match(rootNameSpace, searchName, searchOptions);
                }

                if (searchOptions.IsFlagSet(SearchOptionT.PluginName))
                    match = match || Match(current.name, searchName, searchOptions);

                if (searchOptions.IsFlagSet(SearchOptionT.AssemblyName)) {
                    Assembly asm = current.GetMainAssembly();
                    match = match || Match(asm?.Name(), searchName, searchOptions);
                }

                if (match) {
                    Log.Info("Found plug-in:" + current.GetModName());
                    return current;
                }
            }
            Log.Info($"plug-in not found: keyword={searchName} options={searchOptions}");
            return null;
        }

        public static bool Match(string name1, string name2, SearchOptionT searchOptions = DefaultsearchOptions) {
            if (string.IsNullOrEmpty(name1)) return false;
            Assertion.Assert((searchOptions & SearchOptionT.AllTargets) != 0);

            if (searchOptions.IsFlagSet(SearchOptionT.CaseInsensetive)) {
                name1 = name1.ToLower();
                name2 = name2.ToLower();
            }
            if (searchOptions.IsFlagSet(SearchOptionT.IgnoreWhiteSpace)) {
                name1 = name1.Replace(" ", "");
                name2 = name2.Replace(" ", "");
            }

            if(Log.VERBOSE)
                Log.Debug($"[MATCHING] : {name1} =? {name2} " + searchOptions);

            if (name1 == name2)
                return true;
            if (searchOptions.IsFlagSet(SearchOptionT.Contains)) {
                if (name1.Contains(name2))
                    return true;
            }
            if (searchOptions.IsFlagSet(SearchOptionT.StartsWidth)) {
                if (name1.StartsWith(name2))
                    return true;
            }
            return false;
        }

        public static bool Matches(PluginInfo plugin, ulong[] searchIds) {
            Assertion.AssertNotNull(plugin);
            if (searchIds == null)
                return false;
            foreach (var id in searchIds) {
                if (id == 0) {
                    Log.Error("unexpected 0 as mod search id");
                    continue;
                }
                if (id == plugin.GetWorkshopID())
                    return true;
            }
            return false;
        }
    }
}
