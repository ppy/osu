using OpenTK.Input;
using System.Collections.Generic;
using osu.Game.Modes.Taiko.Judgements;
using System;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        /// <summary>
        /// A list of keys which this HitObject will accept. These are the standard Taiko keys for now.
        /// These should be moved to bindings later.
        /// </summary>
        private List<Key> validKeys = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private DrumRollTick tick;

        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
            this.tick = tick;
        }

        protected override TaikoJudgementInfo CreateJudgementInfo() => new TaikoDrumRollTickJudgementInfo();

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > tick.TickTimeDistance / 2)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            if (Math.Abs(Judgement.TimeOffset) < tick.TickTimeDistance / 2)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.Score = TaikoScoreResult.Great;
            }
        }

        protected override void Update()
        {
            // Drum roll ticks shouldn't move
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat)
                return false;

            if (Judgement.Result.HasValue)
                return false;

            if (!validKeys.Contains(args.Key))
                return false;

            return UpdateJudgement(true);
        }
    }
}
