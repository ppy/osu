// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Timing
{
    internal class WaveformComparisonDisplay : CompositeDrawable
    {
        private const int total_waveforms = 8;

        private const float corner_radius = LabelledDrawable<Drawable>.CORNER_RADIUS;

        private readonly BindableNumber<double> beatLength = new BindableDouble();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private Bindable<ControlPointGroup?> selectedGroup { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private TimingControlPoint timingPoint = TimingControlPoint.DEFAULT;

        private double displayedTime;

        private double selectedGroupStartTime;
        private double selectedGroupEndTime;

        private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

        private readonly BindableBool displayLocked = new BindableBool();

        private LockedOverlay lockedOverlay = null!;

        public WaveformComparisonDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            CornerRadius = corner_radius;
            Masking = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            for (int i = 0; i < total_waveforms; i++)
            {
                AddInternal(new WaveformRow
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Height = 1f / total_waveforms,
                    Y = (float)i / total_waveforms,
                });
            }

            AddInternal(new Circle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.White,
                RelativeSizeAxes = Axes.Y,
                Width = 3,
            });

            AddInternal(lockedOverlay = new LockedOverlay());

            selectedGroup.BindValueChanged(_ => updateTimingGroup(), true);

            controlPointGroups.BindTo(editorBeatmap.ControlPointInfo.Groups);
            controlPointGroups.BindCollectionChanged((_, __) => updateTimingGroup());

            beatLength.BindValueChanged(_ => regenerateDisplay(), true);

            displayLocked.BindValueChanged(locked =>
            {
                lockedOverlay.FadeTo(locked.NewValue ? 1 : 0, 200, Easing.OutQuint);
            }, true);
        }

        private void updateTimingGroup()
        {
            beatLength.UnbindBindings();

            var tcp = selectedGroup.Value?.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault();

            if (tcp == null)
            {
                timingPoint = new TimingControlPoint();
                // During movement of a control point's offset, this clause can be hit momentarily.
                // We don't want to reset the `selectedGroupStartTime` here as we rely on having the
                // last value to perform an offset traversal below.
                selectedGroupEndTime = beatmap.Value.Track.Length;
                return;
            }

            timingPoint = tcp;
            beatLength.BindTo(timingPoint.BeatLengthBindable);

            double? newStartTime = selectedGroup.Value?.Time;

            if (newStartTime.HasValue && selectedGroupStartTime != newStartTime)
            {
                // The offset of the selected point may have changed.
                // This handles the case the user has locked the view and expects the display to update with this change.
                showFromTime(displayedTime + (newStartTime.Value - selectedGroupStartTime));
            }

            var nextGroup = editorBeatmap.ControlPointInfo.TimingPoints
                                         .SkipWhile(g => g != tcp)
                                         .Skip(1)
                                         .FirstOrDefault();

            selectedGroupStartTime = newStartTime ?? 0;
            selectedGroupEndTime = nextGroup?.Time ?? beatmap.Value.Track.Length;
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!displayLocked.Value)
            {
                float trackLength = (float)beatmap.Value.Track.Length;
                int totalBeatsAvailable = (int)(trackLength / timingPoint.BeatLength);

                Scheduler.AddOnce(showFromBeat, (int)(e.MousePosition.X / DrawWidth * totalBeatsAvailable));
            }

            return base.OnMouseMove(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            displayLocked.Toggle();
            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (!IsHovered && !displayLocked.Value)
            {
                int currentBeat = (int)Math.Floor((editorClock.CurrentTimeAccurate - selectedGroupStartTime) / timingPoint.BeatLength);

                showFromBeat(currentBeat);
            }
        }

        private void showFromBeat(int beatIndex) =>
            showFromTime(selectedGroupStartTime + beatIndex * timingPoint.BeatLength);

        private void showFromTime(double time)
        {
            if (displayedTime == time)
                return;

            displayedTime = time;
            regenerateDisplay();
        }

        private void regenerateDisplay()
        {
            double index = (displayedTime - selectedGroupStartTime) / timingPoint.BeatLength;

            // Chosen as a pretty usable number across all BPMs.
            // Optimally we'd want this to scale with the BPM in question, but performing
            // scaling of the display is both expensive in resampling, and decreases usability
            // (as it is harder to track the waveform when making realtime adjustments).
            const float visible_width = 300;

            float trackLength = (float)beatmap.Value.Track.Length;
            float scale = trackLength / visible_width;

            const int start_offset = total_waveforms / 2;

            // Start displaying from before the current beat
            index -= start_offset;

            foreach (var row in InternalChildren.OfType<WaveformRow>())
            {
                // offset to the required beat index.
                double time = selectedGroupStartTime + index * timingPoint.BeatLength;

                float offset = (float)(time - visible_width / 2) / trackLength * scale;

                row.Alpha = time < selectedGroupStartTime || time > selectedGroupEndTime ? 0.2f : 1;
                row.WaveformOffset = -offset;
                row.WaveformScale = new Vector2(scale, 1);
                row.BeatIndex = (int)Math.Floor(index);

                index++;
            }
        }

        internal class LockedOverlay : CompositeDrawable
        {
            private OsuSpriteText text = null!;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = corner_radius;
                BorderColour = colours.Red;
                BorderThickness = 3;
                Alpha = 0;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        AlwaysPresent = true,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                    new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colours.Red,
                                RelativeSizeAxes = Axes.Both,
                            },
                            text = new OsuSpriteText
                            {
                                Colour = colours.GrayF,
                                Text = "Locked",
                                Margin = new MarginPadding(5),
                                Shadow = false,
                                Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold),
                            }
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                text
                    .FadeIn().Then().Delay(500)
                    .FadeOut().Then().Delay(500)
                    .Loop();
            }
        }

        internal class WaveformRow : CompositeDrawable
        {
            private OsuSpriteText beatIndexText = null!;
            private WaveformGraph waveformGraph = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load(IBindable<WorkingBeatmap> beatmap)
            {
                InternalChildren = new Drawable[]
                {
                    waveformGraph = new WaveformGraph
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Both,
                        Waveform = beatmap.Value.Waveform,
                        Resolution = 1,

                        BaseColour = colourProvider.Colour0,
                        LowColour = colourProvider.Colour1,
                        MidColour = colourProvider.Colour2,
                        HighColour = colourProvider.Colour4,
                    },
                    beatIndexText = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Padding = new MarginPadding(5),
                        Colour = colourProvider.Content2
                    }
                };
            }

            public int BeatIndex { set => beatIndexText.Text = value.ToString(); }
            public Vector2 WaveformScale { set => waveformGraph.Scale = value; }
            public float WaveformOffset { set => waveformGraph.X = value; }
        }
    }
}
