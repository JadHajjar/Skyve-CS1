using ColossalFramework.UI;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KianCommons.UI {
    internal static class UIExtensions {
        public static void FitToScreen(this UIComponent target) {
            var pos0 = target.absolutePosition;
            Log.Called($"target={target} absolutePosition={pos0}" );
            Log.Debug(System.Environment.StackTrace, false);
            Vector2 resolution = target.GetUIView().GetScreenResolution();
            var w = target.width;
            var h = target.height;
            var pos1 = new Vector3(
                Mathf.Clamp(pos0.x, 0, resolution.x - w),
                Mathf.Clamp(pos0.y, 0, resolution.y - h));
            Log.Debug($"resolution.x - w = {resolution.x} - {w} = {resolution.x - w}\n" +
                $"Clamp(pos0.x, 0, resolution.x - w) = Clamp({pos0.x}, 0, {resolution.x - w}) = {Mathf.Clamp(pos0.x, 0, resolution.x - w)}");
            Log.Debug($"pos1= {pos1}");
            target.absolutePosition = pos1;
            Log.Info($"target.absolutePosition={target.absolutePosition}, resolution={resolution}");
            target.MakePixelPerfect();
        }

        public static T AddUIComponent<T>(this UIView view) where T : UIComponent {
            return view.AddUIComponent(typeof(T)) as T;
        }

        public static void DestroyFull(this UIComponent c) {
            c.SetAllDeclaredFieldsToNull();
            GameObject.Destroy(c.gameObject);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool FPSOptimisedIsVisble(this UIComponent c) => c.isVisible;
    }
}
