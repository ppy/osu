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
using osu.Game.Graphics;
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

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel), nameof(SkinnableComponentStrings.ShowLabelDescription))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.ShowMaxJudgement))]
        public BindableBool ShowMaxJudgement { get; } = new BindableBool(true);

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.JudgementDisplayMode))]
        public Bindable<DisplayMode> Mode { get; } = new Bindable<DisplayMode>();

        [SettingSource(typeof(JudgementCounterDisplayStrings), nameof(JudgementCounterDisplayStrings.FlowDirection))]
        public Bindable<Direction> FlowDirection { get; } = new Bindable<Direction>();

        protected FillFlowContainer<ArgonJudgementCounter> CounterFlow = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
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
                ArgonJudgementCounter counterComponent = new ArgonJudgementCounter(counter);
                counterComponent.TextComponent.WireframeOpacity.BindTo(WireframeOpacity);
                counterComponent.TextComponent.ShowLabel.BindTo(ShowLabel);
                counterComponent.DisplayedValue.BindTo(counter.ResultCount);
                CounterFlow.Add(counterComponent);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Mode.BindValueChanged(_ => updateVisibility(), true);
            ShowMaxJudgement.BindValueChanged(_ => updateVisibility(), true);
            FlowDirection.BindValueChanged(d => CounterFlow.Direction = getFillDirection(d.NewValue));
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
        }

        private bool shouldBeVisible(int index, ArgonJudgementCounter counter)
        {
            if (index == 0 && !ShowMaxJudgement.Value)
                return false;

            var hitResult = counter.JudgementCounter.Types.First();
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
                    throw new ArgumentOutOfRangeException();
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
