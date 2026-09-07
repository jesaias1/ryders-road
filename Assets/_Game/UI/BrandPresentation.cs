using UnityEngine;

namespace Avoidance.UI
{
    public static class BrandPresentation
    {
        public const string PlayerFacingTitle = "RYDER'S ROAD";
        public const string ProductName = "Ryder's Road";
        public const string LegacyCodename = "RYDERS BLOCK";
        public const string AndroidPackageIdentifier = "com.rydersblockstudio.rydersblock";
        public const string LogoResourcePath = "Branding/RydersRoad_Logo_UI";

        private static Sprite _logoSprite;

        public static Color DeepNavy => new Color(0.015f, 0.035f, 0.09f, 0.94f);
        public static Color PanelNavy => new Color(0.035f, 0.065f, 0.16f, 0.86f);
        public static Color Cyan => new Color(0.18f, 0.86f, 1f, 0.95f);
        public static Color Gold => new Color(1f, 0.66f, 0.13f, 0.95f);
        public static Color MutedWhite => new Color(0.86f, 0.94f, 1f, 0.86f);

        public static Sprite LoadLogoSprite()
        {
            if (_logoSprite != null)
            {
                return _logoSprite;
            }

            var texture = Resources.Load<Texture2D>(LogoResourcePath);
            if (texture == null)
            {
                return null;
            }

            _logoSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect);
            _logoSprite.name = "RydersRoad_Logo_UI_Sprite";
            _logoSprite.hideFlags = HideFlags.HideAndDontSave;
            return _logoSprite;
        }
    }
}
