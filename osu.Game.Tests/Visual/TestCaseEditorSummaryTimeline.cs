// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using OpenTK;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Framework.Configuration;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorSummaryTimeline : OsuTestCase
    {
        private const int length = 60000;
        private readonly Random random;

        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(SummaryTimeline) };

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        public TestCaseEditorSummaryTimeline()
        {
            random = new Random(1337);

            SummaryTimeline summaryTimeline;
            Add(summaryTimeline = new SummaryTimeline
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 50)
            });

            summaryTimeline.Beatmap.BindTo(beatmap);

            AddStep("New beatmap", newBeatmap);

            newBeatmap();
        }

        private void newBeatmap()
        {
            var b = new Beatmap();

            for (int i = 0; i < random.Next(1, 10); i++)
                b.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { Time = random.Next(0, length) });

            for (int i = 0; i < random.Next(1, 5); i++)
                b.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint { Time = random.Next(0, length) });

            for (int i = 0; i < random.Next(1, 5); i++)
                b.ControlPointInfo.EffectPoints.Add(new EffectControlPoint { Time = random.Next(0, length) });

            for (int i = 0; i < random.Next(1, 5); i++)
                b.ControlPointInfo.SamplePoints.Add(new SampleControlPoint { Time = random.Next(0, length) });

            b.BeatmapInfo.Bookmarks = new int[random.Next(10, 30)];
            for (int i = 0; i < b.BeatmapInfo.Bookmarks.Length; i++)
                b.BeatmapInfo.Bookmarks[i] = random.Next(0, length);

            beatmap.Value = new TestWorkingBeatmap(b);
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
