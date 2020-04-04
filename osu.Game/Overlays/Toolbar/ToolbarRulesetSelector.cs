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

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarRulesetSelector : RulesetSelector
    {
        protected Drawable ModeButtonLine { get; private set; }

        public ToolbarRulesetSelector()
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new[]
            {
                new OpaqueBackground
                {
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(disabled => this.FadeColour(disabled ? Color4.Gray : Color4.White, 300), true);
            Current.BindValueChanged(_ => moveLineToCurrent(), true);
        }

        private bool hasInitialPosition;

        // Scheduled to allow the flow layout to be computed before the line position is updated
        private void moveLineToCurrent() => ScheduleAfterChildren(() =>
        {
            if (SelectedTab != null)
            {
                ModeButtonLine.MoveToX(SelectedTab.DrawPosition.X, !hasInitialPosition ? 0 : 200, Easing.OutQuint);
                hasInitialPosition = true;
            }
        });

        public override bool HandleNonPositionalInput => !Current.Disabled && base.HandleNonPositionalInput;

        public override bool HandlePositionalInput => !Current.Disabled && base.HandlePositionalInput;

        public override bool PropagatePositionalInputSubTree => !Current.Disabled && base.PropagatePositionalInputSubTree;

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new ToolbarRulesetTabButton(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Direction = FillDirection.Horizontal,
        };

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
    }
}
