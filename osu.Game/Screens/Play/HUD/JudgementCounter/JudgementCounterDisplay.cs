// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD.JudgementCounter
{
    public partial class JudgementCounterDisplay : CompositeDrawable, ISerialisableDrawable
    {
        public const int TRANSFORM_DURATION = 250;

        public bool UsesFixedAnchor { get; set; }

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayMode))]
        public Bindable<DisplayMode> Mode { get; set; } = new Bindable<DisplayMode>();

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.FlowDirection))]
        public Bindable<Direction> FlowDirection { get; set; } = new Bindable<Direction>();

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.ShowJudgementNames))]
        public BindableBool ShowJudgementNames { get; set; } = new BindableBool(true);

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.ShowMaxJudgement))]
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

            Mode.BindValueChanged(_ => updateDisplay());
            ShowMaxJudgement.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            for (int i = 0; i < CounterFlow.Children.Count; i++)
            {
                JudgementCounter counter = CounterFlow.Children[i];

                if (shouldShow(i, counter))
                    counter.Show();
                else
                    counter.Hide();
            }

            bool shouldShow(int index, JudgementCounter counter)
            {
                if (index == 0 && !ShowMaxJudgement.Value)
                    return false;

                if (counter.Result.Type.IsBasic())
                    return true;

                switch (Mode.Value)
                {
                    case DisplayMode.Simple:
                        return false;

                    case DisplayMode.Normal:
                        return !counter.Result.Type.IsBonus();

                    case DisplayMode.All:
                        return true;

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
            [LocalisableDescription(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayModeSimple))]
            Simple,

            [LocalisableDescription(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayModeNormal))]
            Normal,

            [LocalisableDescription(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayModeAll))]
            All
        }
    }
}
