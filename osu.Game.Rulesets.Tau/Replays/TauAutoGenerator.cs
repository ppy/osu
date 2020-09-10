using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Tau.Objects;
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

        private const float offset = 768 / 2f;
        private const float cursorDistance = 250;

        public override Replay Generate()
        {
            //add some frames at the beginning so the cursor doesnt suddenly appear on the first note
            Replay.Frames.Add(new TauReplayFrame(-100000, new Vector2(offset, offset + 150)));
            Replay.Frames.Add(new TauReplayFrame(Beatmap.HitObjects[0].StartTime - reactionTime, new Vector2(offset, offset + 150)));

            float prevAngle = 0;

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                TauHitObject h = Beatmap.HitObjects[i];
                double releaseDelay = KEY_UP_DELAY;

                if (i + 1 < Beatmap.HitObjects.Count)
                    releaseDelay = Math.Min(KEY_UP_DELAY, Beatmap.HitObjects[i + 1].StartTime - h.StartTime);

                switch (h)
                {
                    case HardBeat _:
                        Replay.Frames.Add(new TauReplayFrame(h.StartTime, ((TauReplayFrame)Replay.Frames.Last()).Position, TauAction.HardButton));
                        Replay.Frames.Add(new TauReplayFrame(h.StartTime + releaseDelay, ((TauReplayFrame)Replay.Frames.Last()).Position));

                        break;

                    case Beat _:
                        //Make the cursor stay at the last note's position if there's enough time between the notes
                        if (i > 0 && h.StartTime - Beatmap.HitObjects[i - 1].StartTime > reactionTime)
                        {
                            Replay.Frames.Add(new TauReplayFrame(h.StartTime - reactionTime, Extensions.GetCircularPosition(cursorDistance, prevAngle) + new Vector2(offset)));

                            buttonIndex = (int)TauAction.LeftButton;
                        }

                        Replay.Frames.Add(new TauReplayFrame(h.StartTime, Extensions.GetCircularPosition(cursorDistance, h.Angle) + new Vector2(offset), (TauAction)(buttonIndex++ % 2)));
                        Replay.Frames.Add(new TauReplayFrame(h.StartTime + releaseDelay, Extensions.GetCircularPosition(cursorDistance, h.Angle) + new Vector2(offset)));
                        prevAngle = h.Angle;

                        break;
                }
            }

            return Replay;
        }
    }
}
