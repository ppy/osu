// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public class DifficultyIcon : DifficultyColouredContainer
    {
        private readonly RulesetInfo ruleset;

        public DifficultyIcon(BeatmapInfo beatmap, RulesetInfo ruleset = null)
            : base(beatmap)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));
            
            this.ruleset = ruleset ?? beatmap.Ruleset;

            Size = new Vector2(20);
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

    public class DifficultyIconWithCounter : FillFlowContainer
    {
        private readonly List<BeatmapInfo> beatmaps;
        private readonly RulesetInfo ruleset;
        private readonly Color4 numberColor;
        private readonly DifficultyType rating;

        private OsuColour palette;

        public DifficultyIconWithCounter(RulesetInfo ruleset, List<BeatmapInfo> beatmaps, Color4 numberColor)
        {
            this.numberColor = numberColor;
            this.beatmaps = beatmaps;
            this.ruleset = ruleset;

            rating = DifficultyRating.GetDifficultyType(beatmaps.OrderBy(b => b.StarDifficulty).Last().StarDifficulty);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour palette)
        {
            this.palette = palette;

            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Direction = FillDirection.Horizontal;
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20),
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
                                Colour = getColour(rating),
                            },
                        },
                        new ConstrainedIconContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Icon = ruleset?.CreateInstance().CreateIcon(),
                        }
                    }
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Right = 5, Top = 3 },
                    Colour = numberColor,
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                    Text = beatmaps.Count.ToString(),
                },
            };
        }

        private Color4 getColour(DifficultyType rating)
        {
            switch (rating)
            {
                case DifficultyType.Easy:
                    return palette.Green;

                case DifficultyType.Normal:
                    return palette.Blue;

                case DifficultyType.Hard:
                    return palette.Yellow;

                case DifficultyType.Insane:
                    return palette.Pink;

                case DifficultyType.Expert:
                    return palette.Purple;

                case DifficultyType.ExpertPlus:
                    return palette.Gray0;

                default:
                    return Color4.Black;
            }
        }
    }
}
