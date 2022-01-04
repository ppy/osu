// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Pippidon.Objects;
using osu.Game.Rulesets.Pippidon.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Pippidon.Replays
{
    public class PippidonAutoGenerator : AutoGenerator<PippidonReplayFrame>
    {
        public new Beatmap<PippidonHitObject> Beatmap => (Beatmap<PippidonHitObject>)base.Beatmap;

        public PippidonAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            int currentLane = 0;

            Frames.Add(new PippidonReplayFrame());

            foreach (PippidonHitObject hitObject in Beatmap.HitObjects)
            {
                if (currentLane == hitObject.Lane)
                    continue;

                int totalTravel = Math.Abs(hitObject.Lane - currentLane);
                var direction = hitObject.Lane > currentLane ? PippidonAction.MoveDown : PippidonAction.MoveUp;

                double time = hitObject.StartTime - 5;

                if (totalTravel == PippidonPlayfield.LANE_COUNT - 1)
                    addFrame(time, direction == PippidonAction.MoveDown ? PippidonAction.MoveUp : PippidonAction.MoveDown);
                else
                {
                    time -= totalTravel * KEY_UP_DELAY;

                    for (int i = 0; i < totalTravel; i++)
                    {
                        addFrame(time, direction);
                        time += KEY_UP_DELAY;
                    }
                }

                currentLane = hitObject.Lane;
            }
        }

        private void addFrame(double time, PippidonAction direction)
        {
            Frames.Add(new PippidonReplayFrame(direction) { Time = time });
            Frames.Add(new PippidonReplayFrame { Time = time + KEY_UP_DELAY }); //Release the keys as well
        }
    }
}
