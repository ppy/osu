// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Screens.Select;
using osuTK.Graphics;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardRankDisplay : Container
    {
        public const int WIDTH = 40;

        public readonly int? Rank;
        public readonly HighlightType? Highlight;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private static readonly Color4 personal_best_gradient_left = Color4Extensions.FromHex("#66FFCC");
        private static readonly Color4 personal_best_gradient_right = Color4Extensions.FromHex("#51A388");

        private const int transition_duration = BeatmapLeaderboardScore.TRANSITION_DURATION;

        private Container highlightGradient = null!;

        private readonly bool sheared;
        private bool firstTransition = true;

        public LeaderboardRankDisplay(int? rank, bool sheared = false, HighlightType? highlight = null)
        {
            Rank = rank;
            Highlight = highlight;

            this.sheared = sheared;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = WIDTH;
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                highlightGradient = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Right = -10f },
                    Alpha = Highlight != null ? 1 : 0,
                    Colour = getHighlightColour(Highlight),
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                },
                new LeaderboardRankLabel(Rank, sheared, darkText: Highlight == HighlightType.Own)
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        private ColourInfo getHighlightColour(HighlightType? highlightType, float lightenAmount = 0)
        {
            switch (highlightType)
            {
                case HighlightType.Own:
                    return ColourInfo.GradientHorizontal(personal_best_gradient_left.Lighten(lightenAmount), personal_best_gradient_right.Lighten(lightenAmount));

                case HighlightType.Friend:
                    return ColourInfo.GradientHorizontal(colours.Pink1.Lighten(lightenAmount), colours.Pink3.Lighten(lightenAmount));

                default:
                    return Colour4.White;
            }
        }

        public void UpdateHighlightState(bool isHovered)
        {
            highlightGradient.FadeColour(getHighlightColour(Highlight, isHovered ? 0.2f : 0), transition_duration, Easing.OutQuint);
        }

        private int transitionDuration() => firstTransition ? 0 : transition_duration;

        public override void Show()
        {
            this.FadeIn(transitionDuration(), Easing.OutQuint)
                .ResizeWidthTo(WIDTH, transitionDuration(), Easing.OutQuint);

            firstTransition = false;
        }

        public override void Hide()
        {
            this.FadeOut(transitionDuration(), Easing.OutQuint)
                .ResizeWidthTo(0, transitionDuration(), Easing.OutQuint);

            firstTransition = false;
        }

        public enum HighlightType
        {
            Own,
            Friend,
        }
    }
}
