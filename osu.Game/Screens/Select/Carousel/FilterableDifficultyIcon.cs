// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class FilterableDifficultyIcon : DifficultyIcon
    {
        private readonly BindableBool filtered = new BindableBool();

        public bool IsFiltered => filtered.Value;

        public readonly CarouselBeatmap Item;

        public FilterableDifficultyIcon(CarouselBeatmap item)
            : base(item.BeatmapInfo)
        {
            filtered.BindTo(item.Filtered);
            filtered.ValueChanged += isFiltered => Schedule(() => this.FadeTo(isFiltered.NewValue ? 0.1f : 1, 100));
            filtered.TriggerChange();

            Item = item;
        }

        protected override bool OnClick(ClickEvent e)
        {
            Item.State.Value = CarouselItemState.Selected;
            return true;
        }
    }
}
