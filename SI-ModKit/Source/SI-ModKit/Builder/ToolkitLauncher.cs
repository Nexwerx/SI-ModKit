using UnityEngine;
using UnityEngine.UI;

namespace SIModKit
{
    internal static class ToolkitLauncher
    {
        private static Button button;
        public static void Create()
        {
            if (button != null || WindowManager.Instance == null) return;
            RectTransform anchor = WindowManager.FindElementPath("MainPanel/Holder/RepPanel/StarProg");
            if (anchor == null) return;
            button = WindowManager.SpawnButton();
            button.name = "SIModKitLauncher";
            Text text = button.GetComponentInChildren<Text>();
            if (text != null) text.text = "ModKit";
            button.onClick.AddListener(BuilderWindow.Toggle);
            // Provisional: after Insights' 264px offset. Replace with launcher registry.
            WindowManager.AddElementToElement(button.gameObject, anchor.gameObject,
                new Rect(375, -34, 102, 32), new Rect(0, 0, 0, 0));
        }
        public static void Destroy()
        {
            if (button != null) UnityEngine.Object.Destroy(button.gameObject);
            button = null;
        }
    }
}
