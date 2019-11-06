// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing
{
    internal class SampleSection : Section<SampleControlPoint>
    {
        private OsuSpriteText bank;
        private OsuSpriteText volume;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new[]
            {
                bank = new OsuSpriteText(),
                volume = new OsuSpriteText(),
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<SampleControlPoint> point)
        {
            bank.Text = $"Bank: {point.NewValue?.SampleBank}";
            volume.Text = $"Volume: {point.NewValue?.SampleVolume}%";
        }

        protected override SampleControlPoint CreatePoint()
        {
            var reference = Beatmap.Value.Beatmap.ControlPointInfo.SamplePointAt(SelectedGroup.Value.Time);

            return new SampleControlPoint
            {
                SampleBank = reference.SampleBank,
                SampleVolume = reference.SampleVolume,
            };
        }
    }
}
