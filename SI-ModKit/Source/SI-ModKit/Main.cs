using UnityEngine;
using UnityEngine.UI;

namespace SIModKit
{
    public class Main : ModMeta
    {
        public override string Name { get { return "SI: ModKit"; } }
        public override void Initialize(ModController.DLLMod parentMod) { base.Initialize(parentMod); }
        public override void ConstructOptionsScreen(RectTransform parent, bool inGame)
        {
            Text label = WindowManager.SpawnLabel();
            if (label == null) return;
            label.text = "SI: ModKit - open the builder with the ModKit button in game.";
            label.color = Color.black;
            WindowManager.AddElementToElement(label.gameObject, parent.gameObject,
                new Rect(8, 8, 540, 35), new Rect(0, 0, 0, 0));
        }
    }
}
