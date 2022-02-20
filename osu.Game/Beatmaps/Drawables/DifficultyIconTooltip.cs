// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    internal class DifficultyIconTooltip : VisibilityContainer, ITooltip<DifficultyIconTooltipContent>
    {
        private readonly OsuSpriteText difficultyName, starRating;
        private readonly Box background;
        private readonly FillFlowContainer difficultyFlow;

        public DifficultyIconTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Children = new Drawable[]
            {
                background = new Box
                {
                    Alpha = 0.9f,
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        difficultyName = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                        },
                        difficultyFlow = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                starRating = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 16, weight: FontWeight.Regular),
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Margin = new MarginPadding { Left = 4 },
                                    Icon = FontAwesome.Solid.Star,
                                    Size = new Vector2(12),
                                },
                            }
                        }
                    }
                }
            };
        }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray3;
        }

        private readonly IBindable<StarDifficulty> starDifficulty = new Bindable<StarDifficulty>();

        public void SetContent(DifficultyIconTooltipContent content)
        {
            difficultyName.Text = content.BeatmapInfo.DifficultyName;

            starDifficulty.UnbindAll();
            starDifficulty.BindTo(content.Difficulty);
            starDifficulty.BindValueChanged(difficulty =>
            {
                starRating.Text = $"{difficulty.NewValue.Stars:0.##}";
                difficultyFlow.Colour = colours.ForStarDifficulty(difficulty.NewValue.Stars);
            }, true);
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
