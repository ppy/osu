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

        private int lastDisplayedBeatIndex;

        private double selectedGroupStartTime;
        private double selectedGroupEndTime;

        private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

        public WaveformComparisonDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            CornerRadius = LabelledDrawable<Drawable>.CORNER_RADIUS;
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

            selectedGroup.BindValueChanged(_ => updateTimingGroup(), true);

            controlPointGroups.BindTo(editorBeatmap.ControlPointInfo.Groups);
            controlPointGroups.BindCollectionChanged((_, __) => updateTimingGroup());

            beatLength.BindValueChanged(_ => showFrom(lastDisplayedBeatIndex), true);
        }

        private void updateTimingGroup()
        {
            beatLength.UnbindBindings();

            selectedGroupStartTime = 0;
            selectedGroupEndTime = beatmap.Value.Track.Length;

            var tcp = selectedGroup.Value?.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault();

            if (tcp == null)
            {
                timingPoint = new TimingControlPoint();
                return;
            }

            timingPoint = tcp;
            beatLength.BindTo(timingPoint.BeatLengthBindable);

            selectedGroupStartTime = selectedGroup.Value?.Time ?? 0;

            var nextGroup = editorBeatmap.ControlPointInfo.TimingPoints
                                         .SkipWhile(g => g != tcp)
                                         .Skip(1)
                                         .FirstOrDefault();

            if (nextGroup != null)
                selectedGroupEndTime = nextGroup.Time;
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            float trackLength = (float)beatmap.Value.Track.Length;
            int totalBeatsAvailable = (int)(trackLength / timingPoint.BeatLength);

            Scheduler.AddOnce(showFrom, (int)(e.MousePosition.X / DrawWidth * totalBeatsAvailable));

            return base.OnMouseMove(e);
        }

        protected override void Update()
        {
            base.Update();

            if (!IsHovered)
            {
                int currentBeat = (int)Math.Floor((editorClock.CurrentTimeAccurate - selectedGroupStartTime) / timingPoint.BeatLength);

                showFrom(currentBeat);
            }
        }

        private void showFrom(int beatIndex)
        {
            if (lastDisplayedBeatIndex == beatIndex)
                return;

            // Chosen as a pretty usable number across all BPMs.
            // Optimally we'd want this to scale with the BPM in question, but performing
            // scaling of the display is both expensive in resampling, and decreases usability
            // (as it is harder to track the waveform when making realtime adjustments).
            const float visible_width = 300;

            float trackLength = (float)beatmap.Value.Track.Length;
            float scale = trackLength / visible_width;

            // Start displaying from before the current beat
            beatIndex -= total_waveforms / 2;

            foreach (var row in InternalChildren.OfType<WaveformRow>())
            {
                // offset to the required beat index.
                double time = selectedGroupStartTime + beatIndex * timingPoint.BeatLength;

                float offset = (float)(time - visible_width / 2) / trackLength * scale;

                row.Alpha = time < selectedGroupStartTime || time > selectedGroupEndTime ? 0.2f : 1;
                row.WaveformOffset = -offset;
                row.WaveformScale = new Vector2(scale, 1);
                row.BeatIndex = beatIndex++;
            }

            lastDisplayedBeatIndex = beatIndex;
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
