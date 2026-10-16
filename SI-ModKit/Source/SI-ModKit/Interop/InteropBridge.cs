using UnityEngine;

namespace SIModKit
{
    /// <summary>
    /// Sandbox-safe interoperability bridge for independently compiled source mods.
    /// Consumers communicate through Unity GameObject.SendMessage using only shared
    /// Unity/string types, so they do not require a compile-time reference to SI ModKit.
    /// </summary>
    public sealed class InteropBridge : MonoBehaviour
    {
        public const string ObjectName = "SIModKit.Bridge";
        public const string CallbackMethod = "OnSIModKitBridgeMessage";

        private UIWindow probeWindow;
        private TextProbeState probeState;

        private sealed class TextProbeState
        {
            public UnityEngine.UI.Text Status;
        }

        public void Ping(string callbackObjectName)
        {
            Reply(callbackObjectName, "PONG|1");
        }

        /// <summary>
        /// Creates a small SI ModKit-owned window proving that a separate mod can request
        /// UI work through the bridge without referencing SI ModKit types at compile time.
        /// </summary>
        public void CreateProbeWindow(string callbackObjectName)
        {
            if (probeWindow != null && probeWindow.NativeWindow != null)
            {
                probeWindow.SetTitle("SI ModKit Interop | bridge connected");
                probeWindow.Show();
                Reply(callbackObjectName, "WINDOW|EXISTING");
                return;
            }

            probeWindow = UI.CreateWindow("SI ModKit Interop | bridge connected", 460, 250);
            probeWindow.SetResizable(false);
            probeWindow.SetContentMargin(14);
            probeWindow.AddHeading("Cross-mod bridge probe");
            probeState = new TextProbeState();
            probeState.Status = probeWindow.AddLabel("A separate source mod reached SI ModKit through Unity messaging.");
            probeWindow.AddButton("Send callback to probe mod", () =>
            {
                if (probeState != null && probeState.Status != null)
                    probeState.Status.text = "Callback sent. Waiting for acknowledgement...";
                Reply(callbackObjectName, "BUTTON|PROBE");
            });
            probeWindow.Show();
            Reply(callbackObjectName, "WINDOW|CREATED");
        }

        /// <summary>Called by the consumer probe after it receives the button callback.</summary>
        public void AcknowledgeProbe(string callbackObjectName)
        {
            if (probeWindow != null && probeWindow.NativeWindow != null)
                probeWindow.SetTitle("SI ModKit Interop | PASS - two-way");
            if (probeState != null && probeState.Status != null)
                probeState.Status.text = "PASS: SI ModKit -> probe callback -> SI ModKit acknowledgement completed.";
            Reply(callbackObjectName, "ACK|PASS");
        }

        private static void Reply(string callbackObjectName, string message)
        {
            if (string.IsNullOrEmpty(callbackObjectName)) return;
            GameObject target = GameObject.Find(callbackObjectName);
            if (target == null) return;
            target.SendMessage(CallbackMethod, message, SendMessageOptions.DontRequireReceiver);
        }
    }
}
