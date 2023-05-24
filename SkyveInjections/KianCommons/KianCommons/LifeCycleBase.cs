namespace KianCommons {
    using ICities;
    using KianCommons.IImplict;
    using System;
    using System.Diagnostics;
    using UnityEngine.SceneManagement;

    public abstract class LifeCycleBase : ILoadingExtension, IMod, IUserMod {
        public static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;
        public static LoadMode Mode => (LoadMode)UpdateMode;
        public static string Scene => SceneManager.GetActiveScene().name;

        public static LifeCycleBase Instance { get; private set; }

        public static Version ModVersion => typeof(LifeCycleBase).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);

        internal LifeCycleBase() => Instance = this;

        #region MOD
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual void OnEnabled() {
            try {
                Log.Debug("Testing StackTrace:\n" + new StackTrace(true).ToString(), copyToGameLog: false);
                UI.TextureUtil.EmbededResources = false;
                Log.VERBOSE = false;

                if(!Helpers.InStartupMenu)
                    HotReload();
            } catch(Exception ex) { Log.Exception(ex); }
        }
        public virtual void OnDisabled() {
            try {
                UnLoad();
                Log.Flush();
            } catch(Exception ex) { Log.Exception(ex); }
        }
        #endregion


        #region LoadingExtension
        public void OnCreated(ILoading _) { }
        public void OnReleased() { }
        public virtual void OnLevelLoaded(LoadMode _) {
            try {
                Load();
            } catch(Exception ex) { Log.Exception(ex); }
        }
        public virtual void OnLevelUnloading() {
            try {
                UnLoad();
            } catch(Exception ex) { Log.Exception(ex); }
        }

        #endregion

        public virtual void HotReload() => Load();
        public abstract void Load();
        public abstract void UnLoad();
    }
}
