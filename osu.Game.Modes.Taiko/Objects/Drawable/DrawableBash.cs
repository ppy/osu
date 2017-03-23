// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System.Collections.Generic;
using osu.Framework.Input;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using System;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableBash : DrawableTaikoHitObject
    {
        /// <summary>
        /// A list of keys which this HitObject will accept. These are the standard Taiko keys for now.
        /// These should be moved to bindings later.
        /// </summary>
        private List<Key> validKeys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        /// <summary>
        /// The amount of times the user has hit this bash.
        /// </summary>
        private int userHits;

        private Bash bash;

        public DrawableBash(Bash bash)
            : base(bash)
        {
            this.bash = bash;

            LifetimeEnd = bash.EndTime;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
            {
                if (Time.Current < HitObject.StartTime)
                    return;

                userHits++;

                if (userHits == bash.RequiredHits)
                {
                    Judgement.Result = HitResult.Hit;
                    Judgement.Score = TaikoScoreResult.Great;
                }
            }
            else
            {
                if (Judgement.TimeOffset < 0)
                    return;

                if (userHits > bash.RequiredHits / 2)
                {
                    Judgement.Result = HitResult.Hit;
                    Judgement.Score = TaikoScoreResult.Good;
                }
                else
                    Judgement.Result = HitResult.Miss;
            }
        }

        protected override void Update()
        {
            UpdateScrollPosition(Math.Min(Time.Current, HitObject.StartTime));
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (Judgement.Result.HasValue)
                return false;

            if (!validKeys.Contains(args.Key))
                return false;

            UpdateJudgement(true);

            return true;
        }

        protected override ScrollingCirclePiece CreateCircle() => new BashCirclePiece();
    }
}
