// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
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

        private Container highlightGradient = null!;

        private readonly bool sheared;

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

        public void UpdateHighlightState(bool isHovered, double duration)
        {
            highlightGradient.FadeColour(getHighlightColour(Highlight, isHovered ? 0.2f : 0), duration, Easing.OutQuint);
        }

        public void Appear(double duration)
        {
            this.FadeIn(duration, Easing.OutQuint).ResizeWidthTo(WIDTH, duration, Easing.OutQuint);
        }

        public void Disappear(double duration)
        {
            this.FadeOut(duration, Easing.OutQuint).ResizeWidthTo(0, duration, Easing.OutQuint);
        }

        public enum HighlightType
        {
            Own,
            Friend,
        }
    }
}
