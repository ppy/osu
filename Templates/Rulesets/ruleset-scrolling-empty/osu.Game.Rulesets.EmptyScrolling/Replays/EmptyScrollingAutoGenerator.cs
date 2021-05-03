﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.EmptyScrolling.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.EmptyScrolling.Replays
{
    public class EmptyScrollingAutoGenerator : AutoGenerator
    {
        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<EmptyScrollingHitObject> Beatmap => (Beatmap<EmptyScrollingHitObject>)base.Beatmap;

        public EmptyScrollingAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        public override Replay Generate()
        {
            Frames.Add(new EmptyScrollingReplayFrame());

            foreach (EmptyScrollingHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new EmptyScrollingReplayFrame
                {
                    Time = hitObject.StartTime
                    // todo: add required inputs and extra frames.
                });
            }

            return Replay;
        }
    }
}
