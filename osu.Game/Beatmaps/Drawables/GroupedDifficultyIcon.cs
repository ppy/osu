// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// A difficulty icon that contains a counter on the right-side of it.
    /// </summary>
    /// <remarks>
    /// Used in cases when there are too many difficulty icons to show.
    /// </remarks>
    public class GroupedDifficultyIcon : DifficultyIcon
    {
        private readonly OsuSpriteText counter;

        private List<BeatmapInfo> beatmaps;

        protected List<BeatmapInfo> Beatmaps
        {
            get => beatmaps;
            set
            {
                beatmaps = value;

                updateDisplay();
            }
        }

        public GroupedDifficultyIcon(List<BeatmapInfo> beatmaps, RulesetInfo ruleset, Color4 counterColour)
            : base(null, ruleset, false)
        {
            this.beatmaps = beatmaps;

            AddInternal(counter = new OsuSpriteText
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Padding = new MarginPadding { Left = Size.X },
                Margin = new MarginPadding { Left = 2, Right = 5 },
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                Colour = counterColour,
            });

            updateDisplay();
        }

        private void updateDisplay()
        {
            if (beatmaps == null || beatmaps.Count == 0)
                return;

            Beatmap = beatmaps.OrderBy(b => b.StarDifficulty).Last();
            counter.Text = beatmaps.Count.ToString();
        }
    }
}
