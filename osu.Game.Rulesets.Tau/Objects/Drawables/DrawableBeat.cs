using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Tau.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.Objects.Drawables
{
    public class DrawableBeat : DrawableTauHitObject, IKeyBindingHandler<TauAction>
    {
        public Container Box;
        public Container IntersectArea;

        private bool validActionPressed;

        public DrawableBeat(Beat hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Size = Vector2.One;

            AddRangeInternal(new Drawable[]
            {
                Box = new Container
                {
                    RelativePositionAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0.05f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        IntersectArea = new Container
                        {
                            Size = new Vector2(16),
                            RelativeSizeAxes = Axes.None,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            AlwaysPresent = true
                        }
                    }
                },
            });

            Position = Vector2.Zero;
        }

        private readonly Bindable<float> size = new Bindable<float>(16); // Change as you see fit.

        [BackgroundDependencyLoader(true)]
        private void load(TauRulesetConfigManager config)
        {
            config?.BindWith(TauRulesetSettings.BeatSize, size);
            size.BindValueChanged(value => Box.Size = new Vector2(value.NewValue), true);

            HitObject.AngleBindable.BindValueChanged(a =>
            {
                Rotation = a.NewValue;
            }, true);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            Box.FadeIn(HitObject.TimeFadeIn);
            Box.MoveToY(-0.485f, HitObject.TimePreempt);
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

            if (CheckValidation.Invoke(this))
            {
                var result = HitObject.HitWindows.ResultFor(timeOffset);

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
                    Box.ScaleTo(2f, time_fade_hit, Easing.OutQuint)
                       .FadeColour(Color4.Yellow, time_fade_hit, Easing.OutQuint)
                       .MoveToOffset(new Vector2(0, -.1f), time_fade_hit, Easing.OutQuint)
                       .FadeOut(time_fade_hit);

                    this.FadeOut(time_fade_hit);

                    break;

                case ArmedState.Miss:
                    Box.ScaleTo(0.5f, time_fade_miss, Easing.InQuint)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(0, -.1f), time_fade_hit, Easing.OutQuint)
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

        public void OnReleased(TauAction action)
        {
        }
    }
}
