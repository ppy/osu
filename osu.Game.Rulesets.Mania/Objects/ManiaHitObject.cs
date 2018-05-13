// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects.Types;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mania.Objects
{
    public abstract class ManiaHitObject : HitObject, IHasColumn, IHasLayer, IHasXPosition
    {
        public virtual int Column { get; set; }

        public virtual int Layer { get; set; }

        public virtual float X
        {
            get => Column; // Somehow has to get the maximum number of columns in the beatmap?
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            HitWindows.AllowsPerfect = true;
            HitWindows.AllowsOk = true;
        }
    }
}
