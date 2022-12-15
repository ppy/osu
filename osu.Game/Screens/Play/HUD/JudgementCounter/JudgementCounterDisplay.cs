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
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Display mode")]
        public Bindable<DisplayMode> Mode { get; set; } = new Bindable<DisplayMode>();

        [SettingSource("Counter direction")]
        public Bindable<Flow> FlowDirection { get; set; } = new Bindable<Flow>();

        [SettingSource("Show judgement names")]
        public BindableBool ShowName { get; set; } = new BindableBool(true);

        [SettingSource("Show max judgement")]
        public BindableBool ShowMax { get; set; } = new BindableBool(true);

        [Resolved]
        private JudgementTally tally { get; set; } = null!;

        protected FillFlowContainer JudgementContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = JudgementContainer = new FillFlowContainer
            {
                Direction = getFlow(FlowDirection.Value),
                Spacing = new Vector2(10),
                AutoSizeAxes = Axes.Both
            };

            foreach (var result in tally.Results)
            {
                JudgementContainer.Add(createCounter(result));
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            FlowDirection.BindValueChanged(direction =>
            {
                JudgementContainer.Direction = getFlow(direction.NewValue);

                //Can't pass directly due to Enum conversion
                foreach (var counter in JudgementContainer.Children.OfType<JudgementCounter>())
                {
                    counter.Direction.Value = getFlow(direction.NewValue);
                }
            }, true);
            Mode.BindValueChanged(_ => updateMode(), true);
            ShowMax.BindValueChanged(value =>
            {
                var firstChild = JudgementContainer.Children.FirstOrDefault();

                if (value.NewValue)
                {
                    firstChild?.Show();
                    return;
                }

                firstChild?.Hide();
            }, true);
        }

        private void updateMode()
        {
            foreach (var counter in JudgementContainer.Children.OfType<JudgementCounter>().Where(counter => !counter.Result.Type.IsBasic()))
            {
                switch (Mode.Value)
                {
                    case DisplayMode.Simple:
                        counter.Hide();

                        break;

                    case DisplayMode.Normal:
                        if (counter.Result.Type.IsBonus())
                        {
                            counter.Hide();
                            break;
                        }

                        counter.Show();
                        break;

                    case DisplayMode.All:
                        counter.Show();

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private FillDirection getFlow(Flow flow)
        {
            switch (flow)
            {
                case Flow.Horizontal:
                    return FillDirection.Horizontal;

                case Flow.Vertical:
                    return FillDirection.Vertical;

                default:
                    throw new ArgumentOutOfRangeException(nameof(flow), flow, @"Unsupported direction");
            }
        }

        //Used to hide default full option in FillDirection
        public enum Flow
        {
            Horizontal,
            Vertical
        }

        public enum DisplayMode
        {
            Simple,
            Normal,
            All
        }

        private JudgementCounter createCounter(JudgementCounterInfo info)
        {
            JudgementCounter counter = new JudgementCounter(info)
            {
                State = { Value = Visibility.Visible },
                ShowName = { BindTarget = ShowName }
            };
            return counter;
        }
    }
}
