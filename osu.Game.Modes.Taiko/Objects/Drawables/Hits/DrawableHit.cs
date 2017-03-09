// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Circle;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Hits
{
    public abstract class DrawableHit : DrawableTaikoHitObject
    {
        /// <summary>
        /// A list of keys which this HitObject will accept.
        /// </summary>
        protected abstract List<Key> Keys { get; }

        /// <summary>
        /// A list of keys which this HitObject will accept. These are the standard Taiko keys for now.
        /// These should be moved to bindings later.
        /// </summary>
        private List<Key> validKeys = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private Container bodyContainer;

        private bool validKeyPressed = true;

        protected DrawableHit(TaikoHitObject hitObject) : base(hitObject)
        {
            CirclePiece bodyPiece;
            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2);

            Children = new Drawable[]
            {
                bodyContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Children = new[]
                    {
                        bodyPiece = CreateBody()
                    }
                }
            };

            bodyPiece.Kiai = hitObject.Kiai;
        }

        /// <summary>
        /// Creates a body circle of this HitCircle.
        /// </summary>
        /// <returns>The body circle.</returns>
        protected abstract CirclePiece CreateBody();

        protected override void CheckJudgement(bool userTriggered)
        {
            TaikoJudgementInfo taikoJudgement = (TaikoJudgementInfo)Judgement;

            if (!userTriggered)
            {
                if (taikoJudgement.TimeOffset > HitObject.HitWindowGood)
                    taikoJudgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(taikoJudgement.TimeOffset);

            if (hitOffset > HitObject.HitWindowMiss)
                return;

            if (!validKeyPressed)
                taikoJudgement.Result = HitResult.Miss;
            else if (hitOffset < HitObject.HitWindowGood)
            {
                taikoJudgement.Result = HitResult.Hit;
                taikoJudgement.Score = hitOffset < HitObject.HitWindowGreat ? TaikoScoreResult.Great : TaikoScoreResult.Good;
            }
            else
                taikoJudgement.Result = HitResult.Miss;
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            base.UpdateState(state);

            switch (State)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    bodyContainer.FadeOut(100);
                    break;
                case ArmedState.Hit:
                    bodyContainer.ScaleTo(0.8f, 400, EasingTypes.OutQuad);

                    bodyContainer.FadeOut(600, EasingTypes.OutQuint);

                    bodyContainer.MoveToY(-200, 250, EasingTypes.Out);
                    bodyContainer.Delay(250);
                    bodyContainer.MoveToY(0, 500, EasingTypes.In);
                    break;
            }
        }

        /// <summary>
        /// Handles a valid taiko keypress.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        /// <returns>The </returns>
        protected virtual bool HandleKeyPress(Key key)
        {
            if (Judgement.Result.HasValue)
                return false;

            validKeyPressed = Keys.Contains(key);

            return UpdateJudgement(true);
        }

        protected sealed override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            // Don't handle held-down keyes
            if (args.Repeat)
                return false;

            // Check if we've pressed a valid taiko key
            if (!validKeys.Contains(args.Key))
                return false;

            // Handle it!
            return HandleKeyPress(args.Key);
        }
    }
}
