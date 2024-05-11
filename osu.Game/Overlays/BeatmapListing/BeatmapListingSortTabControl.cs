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
    public partial class BeatmapListingSortTabControl : OverlaySortTabControl<SortCriteria>
    {
        public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>(Overlays.SortDirection.Descending);

        private (SearchCategory category, bool hasQuery)? currentParameters;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (currentParameters == null)
                Reset(SearchCategory.Leaderboard, false);

            Current.BindValueChanged(_ => SortDirection.Value = Overlays.SortDirection.Descending);
        }

        public void Reset(SearchCategory category, bool hasQuery)
        {
            var newParameters = (category, hasQuery);

            if (currentParameters != newParameters)
            {
                TabControl.Clear();

                TabControl.AddItem(SortCriteria.Title);
                TabControl.AddItem(SortCriteria.Artist);
                TabControl.AddItem(SortCriteria.Difficulty);

                if (category == SearchCategory.Any || category > SearchCategory.Loved)
                    TabControl.AddItem(SortCriteria.Updated);

                if (category < SearchCategory.Pending || category == SearchCategory.Mine)
                    TabControl.AddItem(SortCriteria.Ranked);

                TabControl.AddItem(SortCriteria.Rating);
                TabControl.AddItem(SortCriteria.Plays);
                TabControl.AddItem(SortCriteria.Favourites);

                if (hasQuery)
                    TabControl.AddItem(SortCriteria.Relevance);

                if (category == SearchCategory.Pending)
                    TabControl.AddItem(SortCriteria.Nominations);
            }

            var nonQueryCriteria = category >= SearchCategory.Pending ? SortCriteria.Updated : SortCriteria.Ranked;

            Current.Value = hasQuery ? SortCriteria.Relevance : nonQueryCriteria;
            SortDirection.Value = Overlays.SortDirection.Descending;

            // if the new criteria isn't different from the previous one,
            // then re-adding tab items will not mark the current tab as selected.
            // see: https://github.com/ppy/osu-framework/issues/5412
            TabControl.Current.TriggerChange();

            currentParameters = newParameters;
        }

        protected override SortTabControl CreateControl() => new BeatmapSortTabControl
        {
            SortDirection = { BindTarget = SortDirection },
        };

        private partial class BeatmapSortTabControl : SortTabControl
        {
            protected override bool AddEnumEntriesAutomatically => false;

            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            protected override TabItem<SortCriteria> CreateTabItem(SortCriteria value) => new BeatmapSortTabItem(value)
            {
                SortDirection = { BindTarget = SortDirection }
            };
        }

        private partial class BeatmapSortTabItem : SortTabItem
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

        public partial class BeatmapTabButton : TabButton
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
                    icon.Icon = direction.NewValue == Overlays.SortDirection.Ascending && Active.Value ? FontAwesome.Solid.CaretUp : FontAwesome.Solid.CaretDown;
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
