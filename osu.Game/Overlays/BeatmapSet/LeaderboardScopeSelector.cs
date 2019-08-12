// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardScopeSelector : PageTabControl<BeatmapLeaderboardScope>
    {
        protected override bool AddAllItemsIfEnum => false;

        protected override Dropdown<BeatmapLeaderboardScope> CreateDropdown() => null;

        protected override TabItem<BeatmapLeaderboardScope> CreateTabItem(BeatmapLeaderboardScope value) => new ScopeSelectorTabItem(value);

        public LeaderboardScopeSelector()
        {
            RelativeSizeAxes = Axes.X;

            AddItem(BeatmapLeaderboardScope.Global);
            AddItem(BeatmapLeaderboardScope.Country);
            AddItem(BeatmapLeaderboardScope.Friend);

            AddInternal(new Line
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            });
        }

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            AutoSizeAxes = Axes.X,
            RelativeSizeAxes = Axes.Y,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(20, 0),
        };

        private class ScopeSelectorTabItem : PageTabItem
        {
            public ScopeSelectorTabItem(BeatmapLeaderboardScope value)
                : base(value)
            {
                Text.Font = OsuFont.GetFont(size: 16);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BoxColour = colours.Blue;
            }

            protected override bool OnHover(HoverEvent e)
            {
                Text.FadeColour(BoxColour);

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                Text.FadeColour(Color4.White);
            }
        }

        private class Line : GridContainer
        {
            public Line()
            {
                Height = 1;
                RelativeSizeAxes = Axes.X;
                Width = 0.8f;
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(mode: GridSizeMode.Relative, size: 0.4f),
                    new Dimension(),
                };
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Transparent, Color4.Gray),
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Gray,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Gray, Color4.Transparent),
                        },
                    }
                };
            }
        }
    }
}
