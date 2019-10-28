// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing
{
    internal class DifficultySection : Section<DifficultyControlPoint>
    {
        private OsuSpriteText multiplier;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new[]
            {
                multiplier = new OsuSpriteText(),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ControlPoint.BindValueChanged(point => { multiplier.Text = $"Multiplier: {point.NewValue?.SpeedMultiplier::0.##}x"; });
        }

        protected override DifficultyControlPoint CreatePoint()
        {
            var reference = Beatmap.Value.Beatmap.ControlPointInfo.DifficultyPointAt(SelectedGroup.Value.Time);

            return new DifficultyControlPoint
            {
                SpeedMultiplier = reference.SpeedMultiplier,
            };
        }
    }
}
