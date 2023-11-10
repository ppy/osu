// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacySongProgress : SongProgress
    {
        private CircularProgress circularProgress = null!;

        // Legacy song progress doesn't support interaction for now.
        public override bool HandleNonPositionalInput => false;
        public override bool HandlePositionalInput => false;

        public LegacySongProgress()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.92f),
                    Child = circularProgress = new CircularProgress
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new CircularContainer
                {
                    Size = new Vector2(33),
                    Masking = true,
                    BorderColour = Colour4.White,
                    BorderThickness = 2,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0,
                    }
                },
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Colour4.White,
                    Size = new Vector2(4),
                }
            };
        }

        protected override void UpdateProgress(double progress, bool isIntro)
        {
            if (isIntro)
            {
                circularProgress.Scale = new Vector2(-1, 1);
                circularProgress.Anchor = Anchor.TopRight;
                circularProgress.Colour = new Colour4(199, 255, 47, 153);
                circularProgress.Current.Value = 1 - progress;
            }
            else
            {
                circularProgress.Scale = new Vector2(1);
                circularProgress.Anchor = Anchor.TopLeft;
                circularProgress.Colour = new Colour4(255, 255, 255, 153);
                circularProgress.Current.Value = progress;
            }
        }
    }
}
