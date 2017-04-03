// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using System.Collections.Generic;
using osu.Framework.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public abstract class DrawableTaikoHitObject : DrawableHitObject<TaikoHitObject, TaikoJudgement>
    {
        /// <summary>
        /// A list of keys which this hit object will accept. These are the standard Taiko keys for now.
        /// These should be moved to bindings later.
        /// </summary>
        private readonly List<Key> validKeys = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        protected DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            RelativePositionAxes = Axes.X;
        }

        protected override void LoadComplete()
        {
            LifetimeStart = HitObject.StartTime - HitObject.PreEmpt * 2;

            base.LoadComplete();
        }

        protected override TaikoJudgement CreateJudgement() => new TaikoJudgement();

        /// <summary>
        /// Sets the scroll position of the DrawableHitObject relative to the offset between
        /// a time value and the HitObject's StartTime.
        /// </summary>
        /// <param name="time"></param>
        protected virtual void UpdateScrollPosition(double time)
        {
            MoveToX((float)((HitObject.StartTime - time) / HitObject.PreEmpt));
        }

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
