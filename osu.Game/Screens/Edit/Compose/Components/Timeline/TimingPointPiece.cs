// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimingPointPiece : TopPointPiece
    {
        private readonly BindableNumber<double> beatLength;

        public TimingPointPiece(TimingControlPoint point)
            : base(point)
        {
            beatLength = point.BeatLengthBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatLength.BindValueChanged(beatLength =>
            {
                Label.Text = $"{60000 / beatLength.NewValue:n1} BPM";
            }, true);
        }
    }
}
