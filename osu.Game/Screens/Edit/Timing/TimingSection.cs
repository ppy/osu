// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing
{
    internal class TimingSection : Section<TimingControlPoint>
    {
        private OsuSpriteText bpm;
        private OsuSpriteText timeSignature;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new[]
            {
                bpm = new OsuSpriteText(),
                timeSignature = new OsuSpriteText(),
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<TimingControlPoint> point)
        {
            bpm.Text = $"BPM: {point.NewValue?.BPM:0.##}";
            timeSignature.Text = $"Signature: {point.NewValue?.TimeSignature}";
        }

        protected override TimingControlPoint CreatePoint()
        {
            var reference = Beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(SelectedGroup.Value.Time);

            return new TimingControlPoint
            {
                BeatLength = reference.BeatLength,
                TimeSignature = reference.TimeSignature
            };
        }
    }
}
