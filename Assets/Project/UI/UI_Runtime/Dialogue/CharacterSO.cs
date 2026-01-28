using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game {
    [CreateAssetMenu(fileName = "CharacterSO", menuName = "CharacterSO")]
    public class CharacterSO : ScriptableObject {
        public string title;
        public Texture2D portrait;
        public Sprite sprite;
        public AudioClip voice;
        public bool hideUIWhenFinished;
    }
}
