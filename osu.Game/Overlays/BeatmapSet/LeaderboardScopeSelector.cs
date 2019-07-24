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

            foreach (var scope in Enum.GetValues(typeof(BeatmapLeaderboardScope)))
            {
                if (scope is BeatmapLeaderboardScope.Local)
                    continue;

                AddItem((BeatmapLeaderboardScope)scope);
            }

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
            protected readonly OsuSpriteText BoldText;

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
                        Text = value.GetDescription() + "Ranking",
                        Font = OsuFont.GetFont(weight: FontWeight.Light),
                        AlwaysPresent = true,
                    },
                    BoldText = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Bottom = 8 },
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                        Text = value.GetDescription() + "Ranking",
                        Font = OsuFont.GetFont(weight: FontWeight.Black),
                        Alpha = 0,
                        AlwaysPresent = true,
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

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                Text.FadeColour(Color4.White);
            }

            protected override void OnActivated()
            {
                box.ScaleTo(new Vector2(1f), transition_duration);
                Text.FadeOut(transition_duration, Easing.OutQuint);
                BoldText.FadeIn(transition_duration, Easing.OutQuint);
            }

            protected override void OnDeactivated()
            {
                box.ScaleTo(new Vector2(1f, 0f), transition_duration);
                BoldText.FadeOut(transition_duration, Easing.OutQuint);
                Text.FadeIn(transition_duration, Easing.OutQuint);
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
