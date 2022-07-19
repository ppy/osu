// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Benchmarks
{
    public class BenchmarkHitObject : BenchmarkTest
    {
        [Params(1, 100, 1000)]
        public int Count { get; set; }

        [Params(false, true)]
        public bool WithBindableAccess { get; set; }

        [Benchmark]
        public HitCircle[] OsuCircle()
        {
            var circles = new HitCircle[Count];

            for (int i = 0; i < Count; i++)
            {
                circles[i] = new HitCircle();

                if (WithBindableAccess)
                {
                    _ = circles[i].PositionBindable;
                    _ = circles[i].ScaleBindable;
                    _ = circles[i].ComboIndexBindable;
                    _ = circles[i].ComboOffsetBindable;
                    _ = circles[i].StackHeightBindable;
                    _ = circles[i].LastInComboBindable;
                    _ = circles[i].ComboIndexWithOffsetsBindable;
                    _ = circles[i].IndexInCurrentComboBindable;
                    _ = circles[i].SamplesBindable;
                    _ = circles[i].StartTimeBindable;
                }
            }

            return circles;
        }

        [Benchmark]
        public Hit[] TaikoHit()
        {
            var hits = new Hit[Count];

            for (int i = 0; i < Count; i++)
            {
                hits[i] = new Hit();

                if (WithBindableAccess)
                {
                    _ = hits[i].TypeBindable;
                    _ = hits[i].IsStrongBindable;
                    _ = hits[i].SamplesBindable;
                    _ = hits[i].StartTimeBindable;
                }
            }

            return hits;
        }

        [Benchmark]
        public Fruit[] CatchFruit()
        {
            var fruit = new Fruit[Count];

            for (int i = 0; i < Count; i++)
            {
                fruit[i] = new Fruit();

                if (WithBindableAccess)
                {
                    _ = fruit[i].OriginalXBindable;
                    _ = fruit[i].XOffsetBindable;
                    _ = fruit[i].ScaleBindable;
                    _ = fruit[i].ComboIndexBindable;
                    _ = fruit[i].HyperDashBindable;
                    _ = fruit[i].LastInComboBindable;
                    _ = fruit[i].ComboIndexWithOffsetsBindable;
                    _ = fruit[i].IndexInCurrentComboBindable;
                    _ = fruit[i].IndexInBeatmapBindable;
                    _ = fruit[i].SamplesBindable;
                    _ = fruit[i].StartTimeBindable;
                }
            }

            return fruit;
        }

        [Benchmark]
        public Note[] ManiaNote()
        {
            var notes = new Note[Count];

            for (int i = 0; i < Count; i++)
            {
                notes[i] = new Note();

                if (WithBindableAccess)
                {
                    _ = notes[i].ColumnBindable;
                    _ = notes[i].SamplesBindable;
                    _ = notes[i].StartTimeBindable;
                }
            }

            return notes;
        }
    }
}
