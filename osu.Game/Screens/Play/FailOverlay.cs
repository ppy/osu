// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Game.Scoring;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play
{
    public partial class FailOverlay : GameplayMenuOverlay
    {
        public Func<Task<ScoreInfo>>? SaveReplay;

        public override LocalisableString Header => GameplayMenuOverlayStrings.FailedHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            // from #10339 maybe this is a better visual effect
            Add(new Container
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = TwoLayerButton.SIZE_EXTENDED.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#333")
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Spacing = new Vector2(5),
                        Padding = new MarginPadding(10),
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            new SaveFailedScoreButton(SaveReplay)
                            {
                                Width = 300
                            },
                        }
                    }
                }
            });
        }
    }
}
