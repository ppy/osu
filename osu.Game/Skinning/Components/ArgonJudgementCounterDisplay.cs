// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD.JudgementCounter;
using osuTK;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class ArgonJudgementCounterDisplay : CompositeDrawable, ISerialisableDrawable
    {
        [Resolved]
        private JudgementCountController judgementCountController { get; set; } = null!;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wireframes behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.25f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.ShowMaxJudgement))]
        public BindableBool ShowMaxJudgement { get; } = new BindableBool(true);

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayMode))]
        public Bindable<DisplayMode> Mode { get; } = new Bindable<DisplayMode>();

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.FlowDirection))]
        public Bindable<Direction> FlowDirection { get; } = new Bindable<Direction>();

        private readonly Bindable<int?> wireframeDigits = new Bindable<int?>();

        protected FillFlowContainer<ArgonJudgementCounter> CounterFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = CounterFlow = new FillFlowContainer<ArgonJudgementCounter>
            {
                Direction = getFillDirection(FlowDirection.Value),
                Spacing = new Vector2(16),
                AutoSizeAxes = Axes.Both,
            };

            foreach (var counter in judgementCountController.Counters)
            {
                counter.ResultCount.BindValueChanged(_ => updateWireframeDigits());
                ArgonJudgementCounter counterComponent = new ArgonJudgementCounter(counter)
                {
                    WireframeOpacity = { BindTarget = WireframeOpacity },
                    WireframeDigits = { BindTarget = wireframeDigits },
                    ShowLabel = { BindTarget = ShowLabel },
                };
                CounterFlow.Add(counterComponent);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Mode.BindValueChanged(_ => updateVisibility());
            ShowMaxJudgement.BindValueChanged(_ => updateVisibility(), true);
            FlowDirection.BindValueChanged(_ => updateFlowDirection(), true);
        }

        private void updateVisibility()
        {
            for (int i = 0; i < CounterFlow.Children.Count; i++)
            {
                ArgonJudgementCounter counter = CounterFlow.Children[i];

                if (shouldBeVisible(i, counter))
                    counter.Show();
                else
                    counter.Hide();
            }

            updateWireframeDigits();
        }

        private void updateFlowDirection()
        {
            CounterFlow.Direction = getFillDirection(FlowDirection.Value);
            updateWireframeDigits();
        }

        private void updateWireframeDigits()
        {
            var visibleCounters = CounterFlow.Children.Where(counter => counter.State.Value == Visibility.Visible).ToArray();

            if (visibleCounters.Length == 0)
                return;

            wireframeDigits.Value = FlowDirection.Value == Direction.Vertical
                ? Math.Max(2, visibleCounters.Max(counter => counter.Result.ResultCount.Value).ToString().Length)
                : null;
        }

        private bool shouldBeVisible(int index, ArgonJudgementCounter counter)
        {
            if (index == 0 && !ShowMaxJudgement.Value)
                return false;

            var hitResult = counter.Result.Types.First();
            if (hitResult.IsBasic())
                return true;

            switch (Mode.Value)
            {
                case DisplayMode.Simple:
                    return false;

                case DisplayMode.Normal:
                    return !hitResult.IsBonus();

                case DisplayMode.All:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Mode), Mode.Value, null);
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
                    throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
            }
        }

        public enum DisplayMode
        {
            [LocalisableDescription(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayModeSimple))]
            Simple,

            [LocalisableDescription(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayModeNormal))]
            Normal,

            [LocalisableDescription(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayModeAll))]
            All
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
