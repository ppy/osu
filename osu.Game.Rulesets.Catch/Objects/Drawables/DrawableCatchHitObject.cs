// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public abstract class DrawableCatchHitObject : DrawableHitObject<CatchHitObject>
    {
        public readonly Bindable<float> XBindable = new Bindable<float>();

        protected override double InitialLifetimeOffset => HitObject.TimePreempt;

        protected override float SamplePlaybackPosition => HitObject.X / CatchPlayfield.WIDTH;

        public int RandomSeed => HitObject?.RandomSeed ?? 0;

        protected DrawableCatchHitObject([CanBeNull] CatchHitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.BottomLeft;
        }

        /// <summary>
        /// Get a random number in range [0,1) based on seed <see cref="RandomSeed"/>.
        /// </summary>
        public float RandomSingle(int series) => StatelessRNG.NextSingle(RandomSeed, series);

        protected override void OnApply()
        {
            base.OnApply();

            XBindable.BindTo(HitObject.XBindable);
        }

        protected override void OnFree()
        {
            base.OnFree();

            XBindable.UnbindFrom(HitObject.XBindable);
        }

        public Func<CatchHitObject, bool> CheckPosition;

        public bool IsOnPlate;

        public override bool RemoveWhenNotAlive => IsOnPlate;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (CheckPosition == null) return;

            if (timeOffset >= 0 && Result != null)
                ApplyResult(r => r.Type = CheckPosition.Invoke(HitObject) ? r.Judgement.MaxResult : r.Judgement.MinResult);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(250).RotateTo(Rotation * 2, 250, Easing.Out);
                    break;

                case ArmedState.Hit:
                    this.FadeOut();
                    break;
            }
        }
    }
}
