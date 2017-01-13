//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes.Osu.Objects
{
    public abstract class OsuHitObject : HitObject
    {
        public Vector2 Position { get; set; }
        public float Scale { get; set; } = 1;
        public virtual Vector2 EndPosition => Position;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            Scale = (1.0f - 0.7f * (beatmap.BeatmapInfo.BaseDifficulty.CircleSize - 5) / 5) / 2;
        }

        [Flags]
        internal enum HitObjectType
        {
            Circle = 1,
            Slider = 2,
            NewCombo = 4,
            Spinner = 8,
            Hold = 128
        }
    }
}
