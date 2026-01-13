using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game {
    [CreateAssetMenu(fileName = "CharacterSO", menuName = "CharacterSO")]
    public class CharacterSO : ScriptableObject {
        public string title;
        public Texture2D portrait;
        public AudioClip voice;
    }
}
