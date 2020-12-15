// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingSortTabControl : OverlaySortTabControl<SortCriteria>
    {
        public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>(Overlays.SortDirection.Descending);

        public BeatmapListingSortTabControl()
        {
            Current.Value = SortCriteria.Ranked;
        }

        protected override SortTabControl CreateControl() => new BeatmapSortTabControl
        {
            SortDirection = { BindTarget = SortDirection }
        };

        private class BeatmapSortTabControl : SortTabControl
        {
            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            protected override TabItem<SortCriteria> CreateTabItem(SortCriteria value) => new BeatmapSortTabItem(value)
            {
                SortDirection = { BindTarget = SortDirection }
            };
        }

        private class BeatmapSortTabItem : SortTabItem
        {
            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            public BeatmapSortTabItem(SortCriteria value)
                : base(value)
            {
            }

            protected override TabButton CreateTabButton(SortCriteria value) => new BeatmapTabButton(value)
            {
                Active = { BindTarget = Active },
                SortDirection = { BindTarget = SortDirection }
            };
        }

        private class BeatmapTabButton : TabButton
        {
            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            protected override Color4 ContentColour
            {
                set
                {
                    base.ContentColour = value;
                    icon.Colour = value;
                }
            }

            private readonly SpriteIcon icon;

            public BeatmapTabButton(SortCriteria value)
                : base(value)
            {
                Add(icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AlwaysPresent = true,
                    Alpha = 0,
                    Size = new Vector2(6)
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SortDirection.BindValueChanged(direction =>
                {
                    icon.Icon = direction.NewValue == Overlays.SortDirection.Ascending ? FontAwesome.Solid.CaretUp : FontAwesome.Solid.CaretDown;
                }, true);
            }

            protected override void UpdateState()
            {
                base.UpdateState();
                icon.FadeTo(Active.Value || IsHovered ? 1 : 0, 200, Easing.OutQuint);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (Active.Value)
                    SortDirection.Value = SortDirection.Value == Overlays.SortDirection.Ascending ? Overlays.SortDirection.Descending : Overlays.SortDirection.Ascending;

                return base.OnClick(e);
            }
        }
    }
}
