// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Overlays.Toolbar;
using osu.Framework.Graphics.UserInterface;
using System.Linq;

namespace osu.Game.Overlays.Settings
{
    public class Sidebar : TabControl<SettingsSection>
    {
        public const float DEFAULT_WIDTH = ToolbarButton.WIDTH;
        public const int EXPANDED_WIDTH = 200;

        protected override Dropdown<SettingsSection> CreateDropdown() => null;

        public Sidebar()
        {
            AutoSizeAxes = Axes.Y;
            Width = DEFAULT_WIDTH;

            Current.BindValueChanged(tab => contract());
        }

        protected override TabItem<SettingsSection> CreateTabItem(SettingsSection value) => new SidebarButton(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Direction = FillDirection.Vertical,
            AllowMultiline = true,
        };

        private SidebarButton lastHoveredButton;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!TabContainer.Children.Any())
                return;

            foreach (var button in TabContainer.Children)
            {
                ((SidebarButton)button).OnHoverAction += queueExpand;
                ((SidebarButton)button).OnHoverLostAction += currentButton =>
                {
                    if (currentButton == lastHoveredButton)
                        contract();
                };
            }
        }

        private ScheduledDelegate expandEvent;
        private ExpandedState state;

        public ExpandedState State
        {
            get => state;
            set
            {
                if (state == value) return;

                state = value;

                switch (state)
                {
                    default:
                        this.ResizeWidthTo(DEFAULT_WIDTH, 500, Easing.OutQuint);
                        break;

                    case ExpandedState.Expanded:
                        this.ResizeWidthTo(EXPANDED_WIDTH, 500, Easing.OutQuint);
                        break;
                }
            }
        }

        private void queueExpand(SidebarButton button)
        {
            lastHoveredButton = button;

            if (State == ExpandedState.Expanded)
                return;

            contract();
            expandEvent = Scheduler.AddDelayed(() => State = ExpandedState.Expanded, 750);
        }

        private void contract()
        {
            expandEvent?.Cancel();
            State = ExpandedState.Contracted;
        }
    }

    public enum ExpandedState
    {
        Contracted,
        Expanded,
    }
}
