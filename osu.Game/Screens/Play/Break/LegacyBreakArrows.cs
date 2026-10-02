// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public partial class LegacyBreakArrows : SkinReloadableDrawable
    {
        private Texture? playWarningArrowTextureFallback;

        private readonly Sprite[] sprites;

        public LegacyBreakArrows()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = sprites = new[]
            {
                new Sprite
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1, 1),
                    X = 128,
                    Y = 160,
                },
                new Sprite
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(-1, 1),
                    X = -128,
                    Y = 160,
                },
                new Sprite
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1, 1),
                    X = 128,
                    Y = -160,
                },
                new Sprite
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(-1, 1),
                    X = -128,
                    Y = -160,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            var defaultLegacySkin = skins.DefaultClassicSkin;

            playWarningArrowTextureFallback = defaultLegacySkin.GetTexture(@"play-warningarrow");
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            Texture? arrowWarningTexture = skin.GetTexture(@"arrow-warning");
            Texture? playWarningArrowTexture = skin.GetTexture(@"play-warningarrow");
            decimal? skinVersion = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value;

            bool newerTextureAvailable = arrowWarningTexture != null;
            var arrowColour = !newerTextureAvailable && skinVersion >= 2.0m ? Colour4.Red : Colour4.White;
            Texture? arrowTexture = newerTextureAvailable ? arrowWarningTexture : playWarningArrowTexture;

            if (arrowTexture == null)
            {
                arrowColour = Colour4.Red;
                arrowTexture = playWarningArrowTextureFallback;
            }

            foreach (var sprite in sprites)
            {
                sprite.Texture = arrowTexture;
                sprite.Size = arrowTexture?.DisplaySize ?? Vector2.Zero;
                sprite.Colour = arrowColour;
            }
        }
    }
}
