using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.Objects.Drawables
{
    public class DrawableHardBeat : DrawableTauHitObject, IKeyBindingHandler<TauAction>
    {
        protected override TauAction[] HitActions { get; set; } = new[]
        {
            TauAction.HardButton
        };

        public readonly CircularContainer Circle;

        public DrawableHardBeat(TauHitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Size = Vector2.Zero;
            Alpha = 0f;

            AddRangeInternal(new Drawable[]
            {
                Circle = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                    Masking = true,
                    BorderThickness = 5,
                    BorderColour = Color4.White,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 1f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        },
                    }
                },
            });

            Position = Vector2.Zero;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            this.FadeIn(HitObject.TimeFadeIn);
            this.ResizeTo(1, HitObject.TimePreempt);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            if (result == HitResult.None)
                return;

            ApplyResult(r => r.Type = result);
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
                    this.ScaleTo(1.25f, time_fade_hit, Easing.OutQuint)
                        .FadeColour(Color4.Yellow, time_fade_hit, Easing.OutQuint)
                        .FadeOut(time_fade_hit);

                    Circle.TransformTo(nameof(Circle.BorderThickness), 0f, time_fade_hit);

                    break;

                case ArmedState.Miss:
                    this.FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                        .ResizeTo(1.1f, time_fade_hit, Easing.OutQuint)
                        .FadeOut(time_fade_miss);

                    Circle.TransformTo(nameof(Circle.BorderThickness), 0f, time_fade_miss, Easing.OutQuint);

                    break;
            }
        }

        public bool OnPressed(TauAction action)
        {
            if (AllJudged)
                return false;

            if (HitActions.Contains(action))
                return UpdateResult(true);

            return false;
        }

        public void OnReleased(TauAction action)
        {
        }
    }
}
