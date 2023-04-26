// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    /// <summary>
    /// A difficulty icon that contains a counter on the right-side of it.
    /// </summary>
    /// <remarks>
    /// Used in cases when there are too many difficulty icons to show.
    /// </remarks>
    public partial class GroupedDifficultyIcon : DifficultyIcon
    {
        public readonly List<CarouselBeatmap> Items;

        public GroupedDifficultyIcon(List<CarouselBeatmap> items, RulesetInfo ruleset)
            : base(items.OrderBy(b => b.BeatmapInfo.StarRating).Last().BeatmapInfo, ruleset)
        {
            Items = items;

            foreach (var item in items)
                item.Filtered.BindValueChanged(_ => Scheduler.AddOnce(updateFilteredDisplay));

            AddInternal(new OsuSpriteText
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Padding = new MarginPadding { Left = Size.X },
                Margin = new MarginPadding { Left = 2, Right = 5 },
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                Text = items.Count.ToString(),
                Colour = Color4.White,
            });

            updateFilteredDisplay();
        }

        protected override bool OnClick(ClickEvent e)
        {
            Items.First().State.Value = CarouselItemState.Selected;
            return true;
        }

        private void updateFilteredDisplay()
        {
            // for now, fade the whole group based on the ratio of hidden items.
            this.FadeTo(1 - 0.9f * ((float)Items.Count(i => i.Filtered.Value) / Items.Count), 100);
        }
    }
}
