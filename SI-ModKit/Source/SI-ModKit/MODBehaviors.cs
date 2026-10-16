using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIModKit
{
    public class ModKitRuntime : ModBehaviour
    {
        internal static ModKitRuntime ActiveRuntime;
        private InteropBridge interopBridge;
        private void Start()
        {
            ActiveRuntime = this;
            EnsureInteropBridge();
            SceneManager.sceneLoaded += SceneLoaded;
            if (SceneManager.GetActiveScene().name == "MainScene") ToolkitLauncher.Create();
        }
        public override void OnActivate()
        {
            ActiveRuntime = this;
            EnsureInteropBridge();
            if (SceneManager.GetActiveScene().name == "MainScene") ToolkitLauncher.Create();
        }

        public override void OnDeactivate() { Shutdown(); DestroyInteropBridge(); ActiveRuntime = null; }
        private void OnDestroy()
        {
            ActiveRuntime = null;
            SceneManager.sceneLoaded -= SceneLoaded;
            Shutdown();
            DestroyInteropBridge();
        }

        private void EnsureInteropBridge()
        {
            if (interopBridge != null) return;
            GameObject bridgeObject = GameObject.Find(InteropBridge.ObjectName);
            if (bridgeObject == null)
            {
                bridgeObject = new GameObject(InteropBridge.ObjectName);
                bridgeObject.transform.SetParent(transform, false);
            }
            interopBridge = bridgeObject.GetComponent<InteropBridge>();
            if (interopBridge == null) interopBridge = bridgeObject.AddComponent<InteropBridge>();
        }

        private void DestroyInteropBridge()
        {
            if (interopBridge == null) return;
            GameObject bridgeObject = interopBridge.gameObject;
            interopBridge = null;
            if (bridgeObject != null) UnityEngine.Object.Destroy(bridgeObject);
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainScene") ToolkitLauncher.Create();
            else Shutdown();
        }
        private void Shutdown()
        {
            BuilderWindow.Destroy();
            ToolkitLauncher.Destroy();
            UI.DestroyAll();
        }
    }
}
