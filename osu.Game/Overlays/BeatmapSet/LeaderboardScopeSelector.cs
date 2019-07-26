// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using System;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardScopeSelector : TabControl<BeatmapLeaderboardScope>
    {
        protected override Dropdown<BeatmapLeaderboardScope> CreateDropdown() => null;

        protected override TabItem<BeatmapLeaderboardScope> CreateTabItem(BeatmapLeaderboardScope value) => new ScopeSelectorTabItem(value);

        public LeaderboardScopeSelector()
        {
            AutoSizeAxes = Axes.Y;
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
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(20, 0),
        };

        private class ScopeSelectorTabItem : TabItem<BeatmapLeaderboardScope>
        {
            private const float transition_duration = 100;

            private readonly Box box;

            protected readonly OsuSpriteText Text;

            public ScopeSelectorTabItem(BeatmapLeaderboardScope value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    Text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Bottom = 8 },
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                        Text = value.GetDescription() + " Ranking",
                        Font = OsuFont.GetFont(weight: FontWeight.Regular),
                    },
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        Scale = new Vector2(1f, 0f),
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                    },
                    new HoverClickSounds()
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                box.Colour = colours.Blue;
            }

            protected override bool OnHover(HoverEvent e)
            {
                Text.FadeColour(Color4.LightSkyBlue);

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                Text.FadeColour(Color4.White);
            }

            protected override void OnActivated()
            {
                box.ScaleTo(new Vector2(1f), transition_duration);
                Text.Font = Text.Font.With(weight: FontWeight.Black);
            }

            protected override void OnDeactivated()
            {
                box.ScaleTo(new Vector2(1f, 0f), transition_duration);
                Text.Font = Text.Font.With(weight: FontWeight.Regular);
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
