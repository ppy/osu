// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public abstract class FilterControl : CompositeDrawable
    {
        protected const float VERTICAL_PADDING = 10;
        protected const float HORIZONTAL_PADDING = 80;

        [Resolved(CanBeNull = true)]
        private Bindable<FilterCriteria> filter { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        private readonly Box tabStrip;
        private readonly SearchTextBox search;
        private readonly PageTabControl<RoomStatusFilter> tabs;

        protected FilterControl()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.25f,
                },
                tabStrip = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Top = VERTICAL_PADDING,
                        Horizontal = HORIZONTAL_PADDING
                    },
                    Children = new Drawable[]
                    {
                        search = new FilterSearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        tabs = new PageTabControl<RoomStatusFilter>
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    }
                }
            };

            tabs.Current.Value = RoomStatusFilter.Open;
            tabs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            filter ??= new Bindable<FilterCriteria>();
            tabStrip.Colour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            search.Current.BindValueChanged(_ => updateFilterDebounced());
            ruleset.BindValueChanged(_ => UpdateFilter());
            tabs.Current.BindValueChanged(_ => UpdateFilter(), true);
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
            criteria.Status = tabs.Current.Value;
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
