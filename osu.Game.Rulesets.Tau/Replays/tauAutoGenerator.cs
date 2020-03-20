// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.UI;
using osuTK;

namespace osu.Game.Rulesets.Tau.Replays
{
    public class TauAutoGenerator : AutoGenerator
    {
        protected Replay Replay;

        public new Beatmap<TauHitObject> Beatmap => (Beatmap<TauHitObject>)base.Beatmap;

        /// <summary>
        /// The "reaction time" in ms between "seeing" a new hit object and moving to "react" to it.
        /// </summary>
        private const double reactionTime = 200;

        public TauAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        /// <summary>
        /// Which button (left or right) to use for the current hitobject.
        /// Even means LMB will be used to click, odd means RMB will be used.
        /// This keeps track of the button previously used for alt/singletap logic.
        /// </summary>
        private int buttonIndex;

        private const float offset = (768 / 2f) * TauPlayfield.UNIVERSAL_SCALE;
        private const float cursorDistance = 250;

        public override Replay Generate()
        {
            //add some frames at the beginning so the cursor doesnt suddenly appear on the first note
            Replay.Frames.Add(new TauReplayFrame(-100000, new Vector2(offset, offset + 150)));
            Replay.Frames.Add(new TauReplayFrame(Beatmap.HitObjects[0].StartTime - reactionTime, new Vector2(offset, offset + 150)));

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                TauHitObject h = Beatmap.HitObjects[i];

                //Make the cursor stay at the last note's position if there's enough time between the notes
                if (i > 0 && h.StartTime - Beatmap.HitObjects[i - 1].StartTime > reactionTime)
                {
                    float b = Beatmap.HitObjects[i - 1].PositionToEnd.GetDegreesFromPosition(new Vector2(5, 5)) * 4 * MathF.PI / 180;

                    Replay.Frames.Add(new TauReplayFrame(h.StartTime - reactionTime, new Vector2(offset - (cursorDistance * MathF.Cos(b)), offset - (cursorDistance * MathF.Sin(b)))));

                    buttonIndex = (int)TauAction.LeftButton;
                }

                float a = h.PositionToEnd.GetDegreesFromPosition(new Vector2(5, 5)) * 4 * MathF.PI / 180;

                Replay.Frames.Add(new TauReplayFrame(h.StartTime, new Vector2(offset - (cursorDistance * MathF.Cos(a)), offset - (cursorDistance * MathF.Sin(a))), (TauAction)(buttonIndex++ % 2)));
            }

            return Replay;
        }
    }
}
