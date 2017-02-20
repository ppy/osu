using System;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;
using OpenTK;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Vitaru.Objects
{
    public abstract class VitaruHitObject : HitObject
    {
        //All of this can probably go once we actually get into making this work with maps
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
            Dart = 1,
            Wave = 2,
            NewCombo = 4,
            DartNewCombo = 5,
            WaveNewCombo = 6,
            Flower = 8,
            ColourHax = 122,
            Hold = 128,
            ManiaLong = 128,
        }
    }
}