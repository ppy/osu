// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public class FilterableGroupedDifficultyIcon : GroupedDifficultyIcon
    {
        public readonly List<CarouselBeatmap> Items;

        public FilterableGroupedDifficultyIcon(List<CarouselBeatmap> items, RulesetInfo ruleset)
            : base(items.Select(i => i.Beatmap).ToList(), ruleset, Color4.White)
        {
            Items = items;

            foreach (var item in items)
                item.Filtered.BindValueChanged(_ => Scheduler.AddOnce(updateFilteredDisplay));

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
