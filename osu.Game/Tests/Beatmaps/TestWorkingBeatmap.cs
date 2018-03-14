// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Tests.Beatmaps
{
    public class TestWorkingBeatmap : WorkingBeatmap
    {
        public TestWorkingBeatmap(RulesetInfo ruleset)
            : this(new TestBeatmap(ruleset))
        {
        }

        public TestWorkingBeatmap(Beatmap beatmap)
            : base(beatmap.BeatmapInfo)
        {
            this.beatmap = beatmap;
        }

        private readonly Beatmap beatmap;
        protected override Beatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null;

        protected override Track GetTrack()
        {
            var lastObject = beatmap.HitObjects.LastOrDefault();
            if (lastObject != null)
                return new TestTrack(((lastObject as IHasEndTime)?.EndTime ?? lastObject.StartTime) + 1000);
            return new TrackVirtual();
        }

        private class TestTrack : TrackVirtual
        {
            public TestTrack(double length)
            {
                Length = length;
            }
        }
    }
}
