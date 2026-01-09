using UnityEngine;

namespace Game.UI {
    [CreateAssetMenu(fileName = "LecternViewSO",
            menuName = "Scriptable Objects/LecternView")]
    public class LecternViewSO : ScriptableObject {
        public string Title;
        [Multiline(3)]
        public string Description;
        public Sprite Icon;
    }
}
