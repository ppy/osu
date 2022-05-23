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

        private OsuSpriteText beatIndexText = null!;

        private readonly BindableNumber<double> beatLength = new BindableDouble();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private Bindable<ControlPointGroup?> selectedGroup { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private TimingControlPoint timingPoint = TimingControlPoint.DEFAULT;

        private int lastDisplayedBeatIndex;

        private double offsetZeroTime => selectedGroup.Value?.Time ?? 0;

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
                AddInternal(new WaveformGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    BaseColour = colourProvider.Colour0,
                    LowColour = colourProvider.Colour1,
                    MidColour = colourProvider.Colour2,
                    HighColour = colourProvider.Colour4,
                    Waveform = beatmap.Value.Waveform,
                    Resolution = 1,
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

            AddInternal(beatIndexText = new OsuSpriteText
            {
                Margin = new MarginPadding(5),
            });

            selectedGroup.BindValueChanged(selectedGroupChanged, true);
            beatLength.BindValueChanged(_ => showFrom(lastDisplayedBeatIndex), true);
        }

        private void selectedGroupChanged(ValueChangedEvent<ControlPointGroup?> group)
        {
            timingPoint = selectedGroup.Value?.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault()
                          ?? new TimingControlPoint();

            beatLength.UnbindBindings();
            beatLength.BindTo(timingPoint.BeatLengthBindable);
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
                int beatOffset = (int)Math.Max(0, ((editorClock.CurrentTime - offsetZeroTime) / timingPoint.BeatLength));

                showFrom(beatOffset);
            }
        }

        private void showFrom(int beatIndex)
        {
            const float visible_width = 300;

            float trackLength = (float)beatmap.Value.Track.Length;
            float scale = trackLength / visible_width;

            beatIndexText.Text = beatIndex.ToString();

            foreach (var waveform in InternalChildren.OfType<WaveformGraph>())
            {
                // offset to the required beat index.
                float offset = (float)(offsetZeroTime + (beatIndex * timingPoint.BeatLength - (visible_width / 2))) / trackLength * scale;

                waveform.X = -offset;
                waveform.Scale = new Vector2(scale, 1);

                beatIndex++;
            }

            lastDisplayedBeatIndex = beatIndex;
        }
    }
}
