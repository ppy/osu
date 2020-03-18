// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Tau.Objects;

namespace osu.Game.Rulesets.Tau.Replays
{
    public class TauAutoGenerator : AutoGenerator
    {
        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<TauHitObject> Beatmap => (Beatmap<TauHitObject>)base.Beatmap;

        public TauAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        public override Replay Generate()
        {
            Frames.Add(new TauReplayFrame());

            foreach (TauHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new TauReplayFrame
                {
                    Time = hitObject.StartTime,
                    // todo: add required inputs and extra frames.
                });
            }

            return Replay;
        }
    }
}
