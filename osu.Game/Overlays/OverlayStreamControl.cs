// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.UserInterface;
using JetBrains.Annotations;

namespace osu.Game.Overlays
{
    public abstract class OverlayStreamControl<T> : TabControl<T>
    {
        protected OverlayStreamControl()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        public void Populate(List<T> streams) => streams.ForEach(AddItem);

        protected override Dropdown<T> CreateDropdown() => null;

        protected override TabItem<T> CreateTabItem(T value) => CreateStreamItem(value).With(item =>
        {
            item.SelectedItem.BindTo(Current);
        });

        [NotNull]
        protected abstract OverlayStreamItem<T> CreateStreamItem(T value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            AllowMultiline = true,
        };

        protected override bool OnHover(HoverEvent e)
        {
            foreach (var streamBadge in TabContainer.Children.OfType<OverlayStreamItem<T>>())
                streamBadge.UserHoveringArea = true;

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            foreach (var streamBadge in TabContainer.Children.OfType<OverlayStreamItem<T>>())
                streamBadge.UserHoveringArea = false;

            base.OnHoverLost(e);
        }
    }
}
