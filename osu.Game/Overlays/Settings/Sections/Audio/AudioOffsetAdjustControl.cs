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
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public partial class AudioOffsetAdjustControl : CompositeDrawable
    {
        public Bindable<double> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableNumberWithCurrent<double> current = new BindableNumberWithCurrent<double>();

        private readonly IBindableList<SessionAverageHitErrorTracker.DataPoint> averageHitErrorHistory = new BindableList<SessionAverageHitErrorTracker.DataPoint>();

        public readonly Bindable<double?> SuggestedOffset = new Bindable<double?>();

        private Container<Circle> notchContainer = null!;
        private SettingsNote hintNote = null!;
        private RoundedButton applySuggestion = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

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
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(SettingsSection.ITEM_SPACING_V2),
                Children = new Drawable[]
                {
                    new SettingsItemV2(new FormSliderBar<double>
                    {
                        Caption = AudioSettingsStrings.AudioOffset,
                        RelativeSizeAxes = Axes.X,
                        Current = { BindTarget = Current },
                        KeyboardStep = 1,
                        LabelFormat = v => $"{v:N0} ms",
                        TooltipFormat = BeatmapOffsetControl.GetOffsetExplanatoryText,
                    }),
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = SettingsPanel.CONTENT_PADDING,
                        Children = new Drawable[]
                        {
                            notchContainer = new Container<Circle>
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.5f,
                                Height = 10,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Padding = new MarginPadding
                                {
                                    Horizontal = FormSliderBar<double>.InnerSlider.NUB_WIDTH / 2
                                },
                            },
                            hintNote = new SettingsNote { RelativeSizeAxes = Axes.X },
                        }
                    },
                    applySuggestion = new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = AudioSettingsStrings.ApplySuggestedOffset,
                        Padding = SettingsPanel.CONTENT_PADDING,
                        Action = () =>
                        {
                            if (SuggestedOffset.Value.HasValue)
                                current.Value = SuggestedOffset.Value.Value;
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
            current.BindValueChanged(_ => updateHintText());
            SuggestedOffset.BindValueChanged(_ => updateHintText(), true);
        }

        private void updateDisplay(object? _, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SessionAverageHitErrorTracker.DataPoint dataPoint in e.NewItems!)
                    {
                        notchContainer.ForEach(n => n.Alpha *= 0.95f);
                        notchContainer.Add(new Circle
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 2,
                            RelativePositionAxes = Axes.X,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = colourProvider.Light1,
                            X = getXPositionForOffset(dataPoint.SuggestedGlobalAudioOffset)
                        });
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (SessionAverageHitErrorTracker.DataPoint dataPoint in e.OldItems!)
                    {
                        var notch = notchContainer.FirstOrDefault(n => n.X == getXPositionForOffset(dataPoint.SuggestedGlobalAudioOffset));
                        Debug.Assert(notch != null);
                        notchContainer.Remove(notch, true);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    notchContainer.Clear();
                    break;
            }

            SuggestedOffset.Value = averageHitErrorHistory.Any() ? Math.Round(averageHitErrorHistory.Average(dataPoint => dataPoint.SuggestedGlobalAudioOffset)) : null;
        }

        private float getXPositionForOffset(double offset) => (float)(Math.Clamp(offset, current.MinValue, current.MaxValue) / (2 * current.MaxValue));

        private void updateHintText()
        {
            if (SuggestedOffset.Value == null)
            {
                applySuggestion.Enabled.Value = false;
                notchContainer.Hide();
                hintNote.Current.Value = new SettingsNote.Data(AudioSettingsStrings.SuggestedOffsetNote, SettingsNote.Type.Informational);
                hintNote.MoveToY(0, 200, Easing.OutQuint);
            }
            else if (Math.Abs(SuggestedOffset.Value.Value - current.Value) < 1)
            {
                applySuggestion.Enabled.Value = false;
                notchContainer.Show();
                hintNote.Current.Value = new SettingsNote.Data(AudioSettingsStrings.SuggestedOffsetCorrect(averageHitErrorHistory.Count), SettingsNote.Type.Informational);
                hintNote.MoveToY(10, 200, Easing.OutQuint);
            }
            else
            {
                applySuggestion.Enabled.Value = true;
                notchContainer.Show();
                hintNote.Current.Value =
                    new SettingsNote.Data(AudioSettingsStrings.SuggestedOffsetValueReceived(averageHitErrorHistory.Count, SuggestedOffset.Value.Value.ToStandardFormattedString(0)),
                        SettingsNote.Type.Informational);
                hintNote.MoveToY(10, 200, Easing.OutQuint);
            }
        }
    }
}
