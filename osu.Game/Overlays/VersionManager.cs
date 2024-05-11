// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class VersionManager : VisibilityContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures, OsuGameBase game)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Alpha = 0;

            FillFlowContainer mainFill;

            Children = new Drawable[]
            {
                mainFill = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                                    Text = game.Name
                                },
                                new OsuSpriteText
                                {
                                    Colour = DebugUtils.IsDebugBuild ? colours.Red : Color4.White,
                                    Text = game.Version
                                },
                            }
                        },
                    }
                }
            };

            if (!game.IsDeployedBuild)
            {
                mainFill.AddRange(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = OsuFont.Numeric.With(size: 12),
                        Colour = colours.Yellow,
                        Text = @"Development Build"
                    },
                    new Sprite
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Texture = textures.Get(@"Menu/dev-build-footer"),
                    },
                });
            }
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
