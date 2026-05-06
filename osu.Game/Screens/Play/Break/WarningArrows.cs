// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public partial class WarningArrows : SkinReloadableDrawable
    {
        private readonly Sprite[] sprites;

        public WarningArrows()
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

        protected override void SkinChanged(ISkinSource skin)
        {
            Texture? arrowWarningTexture = skin.GetTexture(@"arrow-warning");
            Texture? playWarningArrowTexture = skin.GetTexture(@"play-warningarrow");
            decimal? skinVersion = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value;

            bool newerTextureAvailable = arrowWarningTexture != null;
            var arrowColour = !newerTextureAvailable && skinVersion >= 2.0m ? Colour4.Red : Colour4.White;
            Texture arrowTexture = newerTextureAvailable ? arrowWarningTexture! : playWarningArrowTexture!;

            foreach (var sprite in sprites)
            {
                sprite.Size = Vector2.Zero;
                sprite.Texture = arrowTexture;
                sprite.Colour = arrowColour;
            }
        }
    }
}
