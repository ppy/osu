// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.EmptyFreeform.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.EmptyFreeform.Replays
{
    public class EmptyFreeformAutoGenerator : AutoGenerator
    {
        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public new Beatmap<EmptyFreeformHitObject> Beatmap => (Beatmap<EmptyFreeformHitObject>)base.Beatmap;

        public EmptyFreeformAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        public override Replay Generate()
        {
            Frames.Add(new EmptyFreeformReplayFrame());

            foreach (EmptyFreeformHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new EmptyFreeformReplayFrame
                {
                    Time = hitObject.StartTime,
                    Position = hitObject.Position,
                    // todo: add required inputs and extra frames.
                });
            }

            return Replay;
        }
    }
}
