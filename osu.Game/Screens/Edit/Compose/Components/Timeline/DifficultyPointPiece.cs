// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class DifficultyPointPiece : TopPointPiece
    {
        private readonly BindableNumber<double> speedMultiplier;

        public DifficultyPointPiece(DifficultyControlPoint point)
            : base(point)
        {
            speedMultiplier = point.SpeedMultiplierBindable.GetBoundCopy();

            Y = Height;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            speedMultiplier.BindValueChanged(multiplier => Label.Text = $"{multiplier.NewValue:n2}x", true);
        }
    }
}
