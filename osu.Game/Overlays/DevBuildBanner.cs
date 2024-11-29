// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class DevBuildBanner : VisibilityContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures, OsuGameBase game)
        {
            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Alpha = 0;

            AddRange(new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Font = OsuFont.Numeric.With(weight: FontWeight.Bold, size: 12),
                    Colour = colours.YellowDark,
                    Text = @"DEVELOPER BUILD",
                },
                new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Texture = textures.Get(@"Menu/dev-build-footer"),
                    Scale = new Vector2(0.4f, 1),
                    Y = 2,
                },
            });
        }

        protected override void PopIn()
        {
            this.FadeIn(1400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(500, Easing.OutQuint);
        }
    }
}
