namespace KianCommons {
    using ColossalFramework;
    using System.Linq;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class SingletonStrong<T> where T : Component {
        private static T instance_;
        public static T Instance =>
            instance_ ??=
            Object.FindObjectsOfType<T>()
            .FirstOrDefault(tool => tool.GetType() == typeof(T))
            ?? Singleton<T>.instance;
        public static void Ensure() => _ = Instance;
    }
}
