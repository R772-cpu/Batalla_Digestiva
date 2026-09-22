using UnityEngine;

namespace BatallaDigestiva
{
    [CreateAssetMenu(menuName = "Batalla Digestiva/Personaje")]
    public sealed class CharacterData : ScriptableObject
    {
        public string productId = "modulex";
        public string displayName = "Modulex";
        public Sprite normalSprite;
        [Tooltip("Opcional. Hasta recibir el aplastado se encoge el sprite normal.")]
        public Sprite hitSprite;
        public Sprite productSprite;
        public Sprite medalSprite;
        [TextArea] public string shortDescription;
        [Tooltip("Usar el contorno de Sprite Editor > Custom Physics Shape como area de toque.")]
        public bool useSpritePhysicsShape = true;
    }
}
