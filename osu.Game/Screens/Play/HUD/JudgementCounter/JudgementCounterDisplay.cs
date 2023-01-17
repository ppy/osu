// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD.JudgementCounter
{
    public partial class JudgementCounterDisplay : CompositeDrawable, ISkinnableDrawable
    {
        public const int TRANSFORM_DURATION = 500;
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Display mode")]
        public Bindable<DisplayMode> Mode { get; set; } = new Bindable<DisplayMode>();

        [SettingSource("Counter direction")]
        public Bindable<Direction> FlowDirection { get; set; } = new Bindable<Direction>();

        [SettingSource("Show judgement names")]
        public BindableBool ShowJudgementNames { get; set; } = new BindableBool(true);

        [SettingSource("Show max judgement")]
        public BindableBool ShowMaxJudgement { get; set; } = new BindableBool(true);

        [Resolved]
        private JudgementTally tally { get; set; } = null!;

        protected FillFlowContainer<JudgementCounter> CounterFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = CounterFlow = new FillFlowContainer<JudgementCounter>
            {
                Direction = getFillDirection(FlowDirection.Value),
                Spacing = new Vector2(10),
                AutoSizeAxes = Axes.Both
            };

            foreach (var result in tally.Results)
                CounterFlow.Add(createCounter(result));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            FlowDirection.BindValueChanged(direction =>
            {
                var convertedDirection = getFillDirection(direction.NewValue);

                CounterFlow.Direction = convertedDirection;

                foreach (var counter in CounterFlow.Children)
                    counter.Direction.Value = convertedDirection;
            }, true);

            Mode.BindValueChanged(_ => updateMode(), true);

            ShowMaxJudgement.BindValueChanged(value =>
            {
                var firstChild = CounterFlow.Children.FirstOrDefault();
                firstChild.FadeTo(value.NewValue ? 1 : 0, TRANSFORM_DURATION, Easing.OutQuint);
            }, true);
        }

        private void updateMode()
        {
            foreach (var counter in CounterFlow.Children)
            {
                if (counter.Result.Type.IsBasic())
                {
                    counter.Show();
                    continue;
                }

                switch (Mode.Value)
                {
                    case DisplayMode.Simple:
                        counter.Hide();
                        break;

                    case DisplayMode.Normal:
                        counter.FadeTo(counter.Result.Type.IsBonus() ? 0 : 1);
                        break;

                    case DisplayMode.All:
                        counter.Show();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private FillDirection getFillDirection(Direction flow)
        {
            switch (flow)
            {
                case Direction.Horizontal:
                    return FillDirection.Horizontal;

                case Direction.Vertical:
                    return FillDirection.Vertical;

                default:
                    throw new ArgumentOutOfRangeException(nameof(flow), flow, @"Unsupported direction");
            }
        }

        private JudgementCounter createCounter(JudgementTally.JudgementCount info) =>
            new JudgementCounter(info)
            {
                State = { Value = Visibility.Hidden },
                ShowName = { BindTarget = ShowJudgementNames }
            };

        public enum DisplayMode
        {
            Simple,
            Normal,
            All
        }
    }
}
