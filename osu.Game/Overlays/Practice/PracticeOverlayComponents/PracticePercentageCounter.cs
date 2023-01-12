// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Practice.PracticeOverlayComponents
{
    public partial class PracticePercentageCounter : OverlayContainer
    {
        private const float panel_height = 35f;

        private const float panel_shear = 0.15f;

        private readonly PracticePlayerLoader loader;

        public PracticePercentageCounter(PracticePlayerLoader loader) =>
            this.loader = loader;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Shear = new Vector2(panel_shear, 0);
            Height = panel_height;
            Width = 235;
            Masking = true;
            CornerRadius = 5;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Green0.Opacity(.5f),
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 220,
                    Masking = true,
                    CornerRadius = 5,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Green0.Opacity(0.5f)
                        },
                        new OsuSpriteText
                        {
                            Text = $"Practicing {Math.Round(loader.CustomStart.Value * 100)}% to {Math.Round(loader.CustomEnd.Value * 100)}%",
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Shadow = true,
                            ShadowColour = new Color4(0, 0, 0, 0.1f),
                            Shear = new Vector2(-panel_shear, 0),
                            Colour = Colour4.White,
                            Padding = new MarginPadding(10),
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.Delay(2500).Then().Schedule(PopOut);
        }

        protected override void PopIn() => this.FadeInFromZero(500, Easing.OutQuint);
        protected override void PopOut() => this.FadeOutFromOne(500, Easing.OutQuint);
    }
}
