﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarRulesetSelector : RulesetSelector
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
                    Masking = true,
                },
                ModeButtonLine = new Container
                {
                    Size = new Vector2(Toolbar.HEIGHT, 3),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Y = -1,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(18, 3),
                        }
                    }
                },
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(_ => Scheduler.AddOnce(currentDisabledChanged));
            currentDisabledChanged();

            Current.BindValueChanged(_ => moveLineToCurrent());
            // Scheduled to allow the button flow layout to be computed before the line position is updated
            ScheduleAfterChildren(moveLineToCurrent);
        }

        private void currentDisabledChanged()
        {
            this.FadeColour(Current.Disabled ? Color4.Gray : Color4.White, 300);
        }

        private bool hasInitialPosition;

        private void moveLineToCurrent()
        {
            if (SelectedTab != null)
            {
                ModeButtonLine.MoveToX(SelectedTab.DrawPosition.X, !hasInitialPosition ? 0 : 500, Easing.OutElasticQuarter);
                hasInitialPosition = true;
            }
        }

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
                    SelectItem(found);
                return true;
            }

            return false;
        }
    }
}
