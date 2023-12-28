// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public partial class AudioOffsetAdjustControl : SettingsItem<double>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            LabelText = AudioSettingsStrings.AudioOffset;
        }

        protected override Drawable CreateControl() => new AudioOffsetPreview();

        private partial class AudioOffsetPreview : CompositeDrawable, IHasCurrentValue<double>
        {
            public Bindable<double> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            private readonly BindableNumberWithCurrent<double> current = new BindableNumberWithCurrent<double>();

            private readonly IBindableList<double> averageHitErrorHistory = new BindableList<double>();

            private readonly Bindable<double?> suggestedOffset = new Bindable<double?>();

            private Container<Box> notchContainer = null!;
            private TextFlowContainer hintText = null!;
            private RoundedButton applySuggestion = null!;

            [BackgroundDependencyLoader]
            private void load(SessionAverageHitErrorTracker hitErrorTracker)
            {
                averageHitErrorHistory.BindTo(hitErrorTracker.AverageHitErrorHistory);

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TimeSlider
                        {
                            RelativeSizeAxes = Axes.X,
                            Current = { BindTarget = Current },
                            KeyboardStep = 1,
                        },
                        notchContainer = new Container<Box>
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 10,
                            Padding = new MarginPadding { Horizontal = Nub.DEFAULT_EXPANDED_SIZE / 2 },
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        hintText = new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 16))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        applySuggestion = new RoundedButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Apply suggested offset",
                            Action = () =>
                            {
                                if (suggestedOffset.Value.HasValue)
                                    current.Value = suggestedOffset.Value.Value;
                                hitErrorTracker.ClearHistory();
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                averageHitErrorHistory.BindCollectionChanged(updateDisplay, true);
                suggestedOffset.BindValueChanged(_ => updateHintText(), true);
            }

            private void updateDisplay(object? _, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (double average in e.NewItems!)
                        {
                            notchContainer.ForEach(n => n.Alpha *= 0.95f);
                            notchContainer.Add(new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = 2,
                                RelativePositionAxes = Axes.X,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                X = getXPositionForAverage(average)
                            });
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (double average in e.OldItems!)
                        {
                            var notch = notchContainer.FirstOrDefault(n => n.X == getXPositionForAverage(average));
                            Debug.Assert(notch != null);
                            notchContainer.Remove(notch, true);
                        }

                        break;

                    case NotifyCollectionChangedAction.Reset:
                        notchContainer.Clear();
                        break;
                }

                suggestedOffset.Value = averageHitErrorHistory.Count < 3 ? null : -averageHitErrorHistory.Average();
            }

            private float getXPositionForAverage(double average) => (float)(Math.Clamp(-average, current.MinValue, current.MaxValue) / (2 * current.MaxValue));

            private void updateHintText()
            {
                hintText.Text = suggestedOffset.Value == null
                    ? @"Play a few beatmaps to receive a suggested offset!"
                    : $@"Based on the last {averageHitErrorHistory.Count} plays, the suggested offset is {suggestedOffset.Value:N0} ms.";
                applySuggestion.Enabled.Value = suggestedOffset.Value != null;
            }
        }
    }
}
