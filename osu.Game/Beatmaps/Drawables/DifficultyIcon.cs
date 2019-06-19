// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class DifficultyIcon : DifficultyColouredContainer, IHasCustomTooltip
    {
        private readonly RulesetInfo ruleset;

        public DifficultyIcon(BeatmapInfo beatmap, RulesetInfo ruleset = null, Boolean shouldShowTooltip = false)
            : base(beatmap)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            this.ruleset = ruleset ?? beatmap.Ruleset;
            TooltipText = shouldShowTooltip ? $"{beatmap.Version}${beatmap.StarDifficulty:0.##}" : String.Empty;

            Size = new Vector2(20);
        }

        public string TooltipText { get; set; }

        public ITooltip GetCustomTooltip() => new DifficultyIconTooltip(AccentColour);

        public class DifficultyIconTooltip : VisibilityContainer, ITooltip
        {
            private readonly OsuSpriteText difficultyName, starRating;
            private readonly Box background;

            public string TooltipText { get; set; }

            public DifficultyIconTooltip(Color4 accentColour)
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
                            new FillFlowContainer
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
                                        Colour = accentColour
                                    },
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Margin = new MarginPadding { Left = 4 },
                                        Icon = FontAwesome.Solid.Star,
                                        Size = new Vector2(12),
                                        Colour = accentColour,
                                    },
                                }
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.GreyCarmineDark;
            }

            public void Refresh()
            {
                var info = TooltipText.Split('$');
                difficultyName.Text = info[0];
                starRating.Text = info[1];
            }

            public void Move(Vector2 pos) => Position = pos;

            protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

            protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load()
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
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = AccentColour,
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
    }
}
