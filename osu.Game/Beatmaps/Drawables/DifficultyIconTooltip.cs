// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    internal class DifficultyIconTooltip : VisibilityContainer, ITooltip<DifficultyIconTooltipContent>
    {
        private OsuSpriteText difficultyName;
        private StarRatingDisplay starRating;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Children = new Drawable[]
            {
                new Box
                {
                    Alpha = 0.9f,
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        difficultyName = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                        },
                        starRating = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            };
        }

        private DifficultyIconTooltipContent displayedContent;

        public void SetContent(DifficultyIconTooltipContent content)
        {
            if (displayedContent != null)
                starRating.Current.UnbindFrom(displayedContent.Difficulty);

            displayedContent = content;

            starRating.Current.BindTarget = displayedContent.Difficulty;
            difficultyName.Text = displayedContent.BeatmapInfo.DifficultyName;
        }

        public void Move(Vector2 pos) => Position = pos;

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }

    internal class DifficultyIconTooltipContent
    {
        public readonly IBeatmapInfo BeatmapInfo;
        public readonly IBindable<StarDifficulty> Difficulty;

        public DifficultyIconTooltipContent(IBeatmapInfo beatmapInfo, IBindable<StarDifficulty> difficulty)
        {
            BeatmapInfo = beatmapInfo;
            Difficulty = difficulty;
        }
    }
}
