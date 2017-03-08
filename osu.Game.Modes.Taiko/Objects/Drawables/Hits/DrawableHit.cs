// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Circle;

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

        private CirclePiece bodyPiece;
        private Container bodyContainer;

        private bool validKeyPressed = true;

        public DrawableHit(TaikoHitObject hitObject)
            : base(hitObject)
        {
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
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > HitObject.HitWindowGood)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            if (hitOffset > HitObject.HitWindowMiss)
                return;

            TaikoJudgementInfo tji = Judgement as TaikoJudgementInfo;

            if (!validKeyPressed)
                Judgement.Result = HitResult.Miss;
            else if (hitOffset < HitObject.HitWindowGood)
            {
                Judgement.Result = HitResult.Hit;

                if (hitOffset < HitObject.HitWindowGreat)
                    tji.Score = TaikoScoreResult.Great;
                else
                    tji.Score = TaikoScoreResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
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
                    bodyContainer.FadeColour(Color4.Red, 100, EasingTypes.OutQuint);
                    bodyContainer.FadeOut(100);
                    break;
                case ArmedState.Hit:
                    bodyContainer.ScaleTo(1.5f, 150, EasingTypes.OutQuint);
                    bodyContainer.FadeOut(150);
                    break;
            }
        }

        protected override void Update()
        {
            // If the HitCircle was hit, make it stop moving
            if (State != ArmedState.Hit)
                base.Update();
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
