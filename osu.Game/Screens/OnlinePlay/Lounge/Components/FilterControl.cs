// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public abstract class FilterControl : CompositeDrawable
    {
        protected readonly FillFlowContainer Filters;

        [Resolved(CanBeNull = true)]
        private Bindable<FilterCriteria> filter { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        private readonly SearchTextBox search;
        private readonly Dropdown<RoomStatusFilter> statusDropdown;

        protected FilterControl()
        {
            RelativeSizeAxes = Axes.X;
            Height = 70;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Children = new Drawable[]
                {
                    search = new FilterSearchTextBox
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.6f,
                    },
                    Filters = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10),
                        Child = statusDropdown = new SlimEnumDropdown<RoomStatusFilter>
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.None,
                            Width = 160,
                        }
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            filter ??= new Bindable<FilterCriteria>();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            search.Current.BindValueChanged(_ => updateFilterDebounced());
            ruleset.BindValueChanged(_ => UpdateFilter());
            statusDropdown.Current.BindValueChanged(_ => UpdateFilter(), true);
        }

        private ScheduledDelegate scheduledFilterUpdate;

        private void updateFilterDebounced()
        {
            scheduledFilterUpdate?.Cancel();
            scheduledFilterUpdate = Scheduler.AddDelayed(UpdateFilter, 200);
        }

        protected void UpdateFilter() => Scheduler.AddOnce(updateFilter);

        private void updateFilter()
        {
            scheduledFilterUpdate?.Cancel();

            var criteria = CreateCriteria();
            criteria.SearchString = search.Current.Value;
            criteria.Status = statusDropdown.Current.Value;
            criteria.Ruleset = ruleset.Value;

            filter.Value = criteria;
        }

        protected virtual FilterCriteria CreateCriteria() => new FilterCriteria();

        public bool HoldFocus
        {
            get => search.HoldFocus;
            set => search.HoldFocus = value;
        }

        public void TakeFocus() => search.TakeFocus();

        private class FilterSearchTextBox : SearchTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = OsuColour.Gray(0.06f);
                BackgroundFocused = OsuColour.Gray(0.12f);
            }
        }
    }
}
