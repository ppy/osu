// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using osu.Game.Rulesets.Tau.UI;

namespace osu.Game.Rulesets.Tau.Objects.Drawables
{
    public class DrawabletauHitObject : DrawableHitObject<TauHitObject>, IKeyBindingHandler<TauAction>
    {
        public Box Box;

        public Func<DrawabletauHitObject, bool> CheckValidation;

        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        public TauAction[] HitActions { get; set; } = new[]
        {
            TauAction.RightButton,
            TauAction.LeftButton,
        };

        /// <summary>
        /// The action that caused this <see cref="DrawabletauHitObject"/> to be hit.
        /// </summary>
        public TauAction? HitAction { get; private set; }

        private bool validActionPressed;

        protected sealed override double InitialLifetimeOffset => HitObject.TimePreempt;

        public DrawabletauHitObject(TauHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(10);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;

            AddInternal(Box = new Box
            {
                EdgeSmoothness = new Vector2(1f),
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Alpha = 0.05f
            });

            hitObject.Angle = hitObject.PositionToEnd.GetDegreesFromPosition(Box.AnchorPosition) * 4;
            Box.Rotation = hitObject.Angle;

            Position = Vector2.Zero;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            var b = HitObject.PositionToEnd.GetDegreesFromPosition(Box.AnchorPosition) * 4;
            var a = b *= (float)(Math.PI / 180);

            Box.FadeIn(HitObject.TimeFadeIn);
            this.MoveTo(new Vector2(-(TauPlayfield.UNIVERSAL_SCALE * 0.8f * (float)Math.Cos(a)), -(TauPlayfield.UNIVERSAL_SCALE * 0.8f * (float)Math.Sin(a))), HitObject.TimePreempt);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (CheckValidation == null) return;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            bool validated = CheckValidation.Invoke(this);

            if (Result != null && validated)
            {
                var result = HitObject.HitWindows.ResultFor(timeOffset);

                if (result >= HitResult.Meh && result <= HitResult.Great)
                    result = HitResult.Great;

                if (result == HitResult.None)
                    return;

                if (!validActionPressed)
                    ApplyResult(r => r.Type = HitResult.Miss);
                else
                    ApplyResult(r => r.Type = result);
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            const double time_fade_hit = 250, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Idle:
                    LifetimeStart = HitObject.StartTime - HitObject.TimePreempt;
                    HitAction = null;

                    break;

                case ArmedState.Hit:
                    var b = HitObject.PositionToEnd.GetDegreesFromPosition(Box.AnchorPosition) * 4;
                    var a = b *= (float)(Math.PI / 180);

                    Box.ScaleTo(2f, time_fade_hit, Easing.OutCubic)
                       .FadeColour(Color4.Yellow, time_fade_hit, Easing.OutCubic)
                       .MoveToOffset(new Vector2(-(50 * (float)Math.Cos(a)), -(50 * (float)Math.Sin(a))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_hit);

                    this.FadeOut(time_fade_hit);

                    break;

                case ArmedState.Miss:
                    var c = HitObject.PositionToEnd.GetDegreesFromPosition(Box.AnchorPosition) * 4;
                    var d = c *= (float)(Math.PI / 180);

                    Box.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(-(50 * (float)Math.Cos(d)), -(50 * (float)Math.Sin(d))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    this.FadeOut(time_fade_miss);

                    break;
            }
        }

        public bool OnPressed(TauAction action)
        {
            if (Judged)
                return false;

            validActionPressed = HitActions.Contains(action);

            var result = UpdateResult(true);

            if (IsHit)
                HitAction = action;

            return result;
        }

        public void OnReleased(TauAction action) { }
    }
}
