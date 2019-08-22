// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class DifficultyIcon : Container, IHasCustomTooltip
    {
        private readonly BeatmapInfo beatmap;
        private readonly RulesetInfo ruleset;

        public DifficultyIcon(BeatmapInfo beatmap, RulesetInfo ruleset = null, bool shouldShowTooltip = true)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            this.beatmap = beatmap;

            this.ruleset = ruleset ?? beatmap.Ruleset;
            if (shouldShowTooltip)
                TooltipContent = beatmap;

            Size = new Vector2(20);
        }

        public string TooltipText { get; set; }

        public ITooltip GetCustomTooltip() => new DifficultyIconTooltip();

        public object TooltipContent { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(0.84f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.08f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                    },
                    Child = iconBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.ForDifficultyRating(beatmap.DifficultyRating),
                    },
                },
                new ConstrainedIconContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    // the null coalesce here is only present to make unit tests work (ruleset dlls aren't copied correctly for testing at the moment)
                    Icon = ruleset?.CreateInstance().CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle }
                }
            };
        }

        private class DifficultyIconTooltip : VisibilityContainer, ITooltip
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

            private OsuColour colours;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;
                background.Colour = colours.Gray3;
            }

            public bool SetContent(object content)
            {
                if (!(content is BeatmapInfo beatmap))
                    return false;

                difficultyName.Text = beatmap.Version;
                starRating.Text = $"{beatmap.StarDifficulty:0.##}";
                difficultyFlow.Colour = colours.ForDifficultyRating(beatmap.DifficultyRating);

                return true;
            }

            public void Move(Vector2 pos) => Position = pos;

            protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

            protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
        }
    }

    public class DifficultyIconWithCounter : Container
    {
        private readonly List<BeatmapInfo> beatmaps;
        private readonly OsuSpriteText text;
        private readonly DifficultyIcon icon;

        protected List<BeatmapInfo> Beatmaps
        {
            set
            {
                if (value?.Any() ?? false)
                {
                    text.Text = value.Count.ToString();
                    icon.Beatmap = value.OrderBy(b => b.StarDifficulty).Last();
                }
            }
        }

        public DifficultyIconWithCounter(RulesetInfo ruleset, List<BeatmapInfo> beatmaps, Color4 numberColor)
        {
            this.beatmaps = beatmaps;

            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding { Right = 6 };
            Children = new Drawable[]
            {
                icon = new DifficultyIcon(beatmaps.OrderBy(b => b.StarDifficulty).Last(), ruleset)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Padding = new MarginPadding { Left = 21 },
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                    Colour = numberColor,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmaps = beatmaps;
        }
    }
}
