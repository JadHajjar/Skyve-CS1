namespace KianCommons {
    using UnityEngine;

    public static class UnityUtil {
        public static T CreateComponent<T>(bool dontDestroyOnLoad) where T : Component =>
            CreateComponent<T>(dontDestroyOnLoad, out GameObject _);

        public static T CreateComponent<T>(bool dontDestroyOnLoad, out GameObject go) where T : Component {
            go = new GameObject(nameof(T), typeof(T));
            if(dontDestroyOnLoad)
                Object.DontDestroyOnLoad(go);
            return go.GetComponent<T>() as T;
        }
    }
}
;