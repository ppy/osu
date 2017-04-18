// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableTaikoHitObject<TaikoHitType> : DrawableHitObject<TaikoHitObject, TaikoJudgement>
        where TaikoHitType : TaikoHitObject
    {
        /// <summary>
        /// A list of keys which this hit object will accept. These are the standard Taiko keys for now.
        /// These should be moved to bindings later.
        /// </summary>
        private readonly List<Key> validKeys = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        public override Vector2 OriginPosition => new Vector2(DrawHeight / 2);

        protected override Container<Drawable> Content => bodyContainer;

        protected readonly TaikoPiece MainPiece;

        private readonly Container bodyContainer;

        public new TaikoHitType HitObject;

        protected DrawableTaikoHitObject(TaikoHitType hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Custom;

            AutoSizeAxes = Axes.Both;

            RelativePositionAxes = Axes.X;

            AddInternal(bodyContainer = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    MainPiece = CreateMainPiece()
                }
            });

            MainPiece.KiaiMode = HitObject.Kiai;

            LifetimeStart = HitObject.StartTime - HitObject.ScrollTime * 2;
        }

        protected override TaikoJudgement CreateJudgement() => new TaikoJudgement();

        protected virtual TaikoPiece CreateMainPiece() => new CirclePiece(HitObject.IsStrong);

        /// <summary>
        /// Sets the scroll position of the DrawableHitObject relative to the offset between
        /// a time value and the HitObject's StartTime.
        /// </summary>
        /// <param name="time"></param>
        protected virtual void UpdateScrollPosition(double time) => X = (float)((HitObject.StartTime - time) / HitObject.ScrollTime);

        protected override void Update()
        {
            UpdateScrollPosition(Time.Current);
        }

        protected virtual bool HandleKeyPress(Key key) => false;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            // Make sure we don't handle held-down keys
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
