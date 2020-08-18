// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK.Input;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarRulesetSelector : RulesetSelector
    {
        protected Drawable ModeButtonLine { get; private set; }

        private RulesetTabDropdown dropdown => (RulesetTabDropdown)Dropdown;

        protected override Dropdown<RulesetInfo> CreateDropdown() => new RulesetTabDropdown();

        private OpaqueBackground background;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSort = true;
            RelativeSizeAxes = Axes.Both;
            AddRangeInternal(new[]
            {
                background = new OpaqueBackground
                {
                    RelativeSizeAxes = Axes.Y,
                    Depth = 1,
                },
                ModeButtonLine = new Container
                {
                    Size = new Vector2(ToolbarButton.WIDTH, 3),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = new Color4(255, 194, 224, 100),
                        Radius = 15,
                        Roundness = 15,
                    },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });

            TabContainer.TabVisibilityChanged += updateVisibility;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(disabled => this.FadeColour(disabled ? Color4.Gray : Color4.White, 300), true);
            Current.BindValueChanged(_ => moveLineToCurrent(false), true);
        }

        private bool hasInitialPosition;

        // Scheduled to allow the flow layout to be computed before the line position is updated
        private void moveLineToCurrent(bool instant) => ScheduleAfterChildren(() =>
        {
            if (SelectedTab != null)
            {
                ModeButtonLine.MoveToX(SelectedTab.DrawPosition.X, !hasInitialPosition || instant ? 0 : 200, Easing.OutQuint);
                hasInitialPosition = true;
            }
        });

        public override bool HandleNonPositionalInput => !Current.Disabled && base.HandleNonPositionalInput;

        public override bool HandlePositionalInput => !Current.Disabled && base.HandlePositionalInput;

        public override bool PropagatePositionalInputSubTree => !Current.Disabled && base.PropagatePositionalInputSubTree;

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new ToolbarRulesetTabButton(value);

        protected override TabFillFlowContainer CreateTabFlow()
        {
            var flow = base.CreateTabFlow();
            flow.Masking = false;
            return flow;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            base.OnKeyDown(e);

            if (e.ControlPressed && !e.Repeat && e.Key >= Key.Number1 && e.Key <= Key.Number9)
            {
                int requested = e.Key - Key.Number1;

                RulesetInfo found = Rulesets.AvailableRulesets.ElementAtOrDefault(requested);
                if (found != null)
                    Current.Value = found;
                return true;
            }

            return false;
        }

        protected override void Update()
        {
            base.Update();
            background.Width = dropdown.Header.IsPresent ? DrawWidth : TabContainer.TabItems.Count() * ToolbarButton.WIDTH;
        }

        private void updateVisibility(TabItem<RulesetInfo> tab, bool visible)
        {
            if (Current.Value == tab.Value && visible == false)
            {
                PerformTabSort(tab);
                moveLineToCurrent(true);
            }
        }

        private class RulesetTabDropdown : OsuTabDropdown<RulesetInfo>
        {
            public new DropdownHeader Header => base.Header;

            [BackgroundDependencyLoader]
            private void load()
            {
                Header.Margin = new MarginPadding { Right = 7 };
                AccentColour = ToolbarRulesetTabButton.ICON_COLOUR;
            }
        }
    }
}
