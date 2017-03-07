// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDon : DrawableHitCircle
    {
        public override Color4 ExplodeColour { get; protected set; }

        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.F, Key.J });

        public DrawableHitCircleDon(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.PinkDarker;
        }

        protected override CirclePiece CreateBody() => new DonCirclePiece();
    }

    public class DrawableHitCircleKatsu : DrawableHitCircle
    {
        public override Color4 ExplodeColour { get; protected set; }

        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.K });

        public DrawableHitCircleKatsu(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.BlueDarker;
        }

        protected override CirclePiece CreateBody() => new KatsuCirclePiece();
    }

    public abstract class DrawableHitCircle : DrawableTaikoHitObject
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

        public DrawableHitCircle(HitCircle hitCircle)
            : base(hitCircle)
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

            bodyPiece.Kiai = hitCircle.Kiai;
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

            // Must be within great range to be hittable/missable
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
                    break;
                case ArmedState.Hit:
                    const double scale_out = 150;

                    bodyContainer.ScaleTo(1.5f, scale_out);
                    bodyContainer.FadeOut(scale_out);
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
