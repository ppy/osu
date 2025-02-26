// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    internal class Slider : HitObject
    {
        public int RepeatCount { get; set; }

        public double Duration { get; set; }

        public string Path { get; set; }

        public Slider(int repeatCount, double duration, string path)
        {
            RepeatCount = repeatCount;
            Duration = duration;
            Path = path;
        }

        public void LimitRepeatCount(int maxRepeatCount)
        {
            if (RepeatCount > maxRepeatCount)
            {
                RepeatCount = maxRepeatCount;
            }
        }

        public override string ToString()
        {
            return $"Slider: {RepeatCount} repeats, Duration: {Duration}ms, Path: {Path}";
        }
    }
}
