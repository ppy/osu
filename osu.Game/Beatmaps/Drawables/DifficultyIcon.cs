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

        private Box iconBox;

        public BeatmapInfo Beatmap
        {
            set => iconBox.Colour = GetColour(value);
        }

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
                    Child = iconBox = new Box
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

    public class DifficultyIconWithCounter : Container
    {
        private readonly List<BeatmapInfo> beatmaps;

        private OsuSpriteText text;
        private DifficultyIcon icon;

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
