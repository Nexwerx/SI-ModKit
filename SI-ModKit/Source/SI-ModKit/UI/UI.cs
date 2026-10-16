using System;
using System.Collections.Generic;
using UnityEngine;

namespace SIModKit
{
    /// <summary>Release-candidate facade used by SI ModKit and the copy-ready source kit.</summary>
    public static class UI
    {
        private static readonly List<UIWindow> Windows = new List<UIWindow>();
        public static UIWindow CreateWindow(string title, int width = 600, int height = 420)
        {
            if (WindowManager.Instance == null)
                throw new InvalidOperationException("SI ModKit: UI unavailable until WindowManager is initialized.");
            UIWindow item = new UIWindow(title, width, height);
            Windows.Add(item);
            return item;
        }
        /// <summary>Shows a native Software Inc. information dialog.</summary>
        public static void ShowMessage(string message)
        {
            WindowManager.SpawnDialog(message ?? "", true, DialogWindow.DialogType.Information);
        }

        /// <summary>Shows a native Software Inc. confirmation dialog. The confirm callback is
        /// invoked only after the user chooses the affirmative button.</summary>
        public static void ShowConfirmation(string message, Action onConfirm, Action onCancel = null, string confirmLabel = "Confirm", string cancelLabel = "Cancel")
        {
            DialogWindow dialog = WindowManager.SpawnDialog();
            dialog.Show(message ?? "", false, DialogWindow.DialogType.Question,
                new KeyValuePair<string, Action>(string.IsNullOrEmpty(confirmLabel) ? "Confirm" : confirmLabel, () =>
                {
                    dialog.Window.Close();
                    if (onConfirm != null) onConfirm();
                }),
                new KeyValuePair<string, Action>(string.IsNullOrEmpty(cancelLabel) ? "Cancel" : cancelLabel, () =>
                {
                    dialog.Window.Close();
                    if (onCancel != null) onCancel();
                }));
        }

        public static void DestroyAll()
        {
            for (int i = Windows.Count - 1; i >= 0; --i) Windows[i].Destroy();
            Windows.Clear();
        }
        internal static void Forget(UIWindow window) { Windows.Remove(window); }
    }
}
