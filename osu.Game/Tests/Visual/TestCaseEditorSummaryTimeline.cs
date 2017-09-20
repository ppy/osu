// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseEditorSummaryTimeline : OsuTestCase
    {
        private const int length = 60000;
        private readonly Random random;

        public override IReadOnlyList<Type> RequiredTypes => new Type[] { typeof(SummaryTimeline) };

        public TestCaseEditorSummaryTimeline()
        {
            random = new Random(1337);

            Add(new SummaryTimeline
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 50)
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            var beatmap = new Beatmap();

            for (int i = 0; i < random.Next(1, 10); i++)
                beatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { Time = random.Next(0, length) });

            for (int i = 0; i < random.Next(1, 5); i++)
                beatmap.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint { Time = random.Next(0, length) });

            for (int i = 0; i < random.Next(1, 5); i++)
                beatmap.ControlPointInfo.EffectPoints.Add(new EffectControlPoint { Time = random.Next(0, length) });

            for (int i = 0; i < random.Next(1, 5); i++)
                beatmap.ControlPointInfo.SoundPoints.Add(new SoundControlPoint { Time = random.Next(0, length) });

            beatmap.BeatmapInfo.Bookmarks = new int[random.Next(10, 30)];
            for (int i = 0; i < beatmap.BeatmapInfo.Bookmarks.Length; i++)
                beatmap.BeatmapInfo.Bookmarks[i] = random.Next(0, length);

            osuGame.Beatmap.Value = new TestWorkingBeatmap(beatmap);
        }

        private class TestWorkingBeatmap : WorkingBeatmap
        {
            private readonly Beatmap beatmap;

            public TestWorkingBeatmap(Beatmap beatmap)
                : base(beatmap.BeatmapInfo)
            {
                this.beatmap = beatmap;
            }

            protected override Texture GetBackground() => null;

            protected override Beatmap GetBeatmap() => beatmap;

            protected override Track GetTrack() => new TestTrack();

            private class TestTrack : TrackVirtual
            {
                public TestTrack()
                {
                    Length = length;
                }
            }
        }
    }
}
