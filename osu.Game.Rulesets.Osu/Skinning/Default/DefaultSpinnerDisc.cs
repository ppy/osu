// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultSpinnerDisc : CompositeDrawable
    {
        private DrawableSpinner drawableSpinner = null!;

        private const float initial_scale = 1.3f;
        private const float idle_alpha = 0.2f;
        private const float tracking_alpha = 0.4f;

        private Color4 normalColour;
        private Color4 completeColour;

        private SpinnerTicks ticks = null!;

        private int wholeRotationCount;
        private readonly BindableBool complete = new BindableBool();

        private SpinnerFill fill = null!;
        private Container mainContainer = null!;
        private SpinnerCentreLayer centre = null!;
        private SpinnerBackgroundLayer background = null!;

        public DefaultSpinnerDisc()
        {
            // we are slightly bigger than our parent, to clip the top and bottom of the circle
            // this should probably be revisited when scaled spinners are a thing.
            Scale = new Vector2(initial_scale);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, DrawableHitObject drawableHitObject)
        {
            drawableSpinner = (DrawableSpinner)drawableHitObject;

            normalColour = colours.BlueDark;
            completeColour = colours.YellowLight;

            InternalChildren = new Drawable[]
            {
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        background = new SpinnerBackgroundLayer(),
                        fill = new SpinnerFill
                        {
                            Alpha = idle_alpha,
                            AccentColour = normalColour
                        },
                        ticks = new SpinnerTicks
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AccentColour = normalColour
                        },
                    }
                },
                centre = new SpinnerCentreLayer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            complete.BindValueChanged(complete => updateDiscColour(complete.NewValue, 200));
            drawableSpinner.ApplyCustomUpdateState += updateStateTransforms;

            updateStateTransforms(drawableSpinner, drawableSpinner.State.Value);
        }

        protected override void Update()
        {
            base.Update();

            complete.Value = Time.Current >= drawableSpinner.Result.TimeCompleted;

            if (complete.Value)
            {
                if (checkNewRotationCount)
                {
                    fill.FinishTransforms(false, nameof(Alpha));
                    fill
                        .FadeTo(tracking_alpha + 0.2f, 60, Easing.OutExpo)
                        .Then()
                        .FadeTo(tracking_alpha, 250, Easing.OutQuint);
                }
            }
            else
            {
                fill.Alpha = (float)Interpolation.Damp(fill.Alpha, drawableSpinner.RotationTracker.Tracking ? tracking_alpha : idle_alpha, 0.98f, (float)Math.Abs(Clock.ElapsedFrameTime));
            }

            const float initial_fill_scale = 0.2f;
            float targetScale = initial_fill_scale + (1 - initial_fill_scale) * drawableSpinner.Progress;

            fill.Scale = new Vector2((float)Interpolation.Lerp(fill.Scale.X, targetScale, Math.Clamp(Math.Abs(Time.Elapsed) / 100, 0, 1)));
            mainContainer.Rotation = drawableSpinner.RotationTracker.Rotation;
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            if (!(drawableHitObject is DrawableSpinner))
                return;

            Spinner spinner = drawableSpinner.HitObject;

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt))
            {
                this.ScaleTo(initial_scale);
                this.RotateTo(0);

                updateDiscColour(false);

                using (BeginDelayedSequence(spinner.TimePreempt / 2))
                {
                    // constant ambient rotation to give the spinner "spinning" character.
                    this.RotateTo((float)(25 * spinner.Duration / 2000), spinner.TimePreempt + spinner.Duration);
                }

                using (BeginDelayedSequence(spinner.TimePreempt + spinner.Duration + drawableHitObject.Result.TimeOffset))
                {
                    switch (state)
                    {
                        case ArmedState.Hit:
                            this.ScaleTo(initial_scale * 1.2f, 320, Easing.Out);
                            this.RotateTo(mainContainer.Rotation + 180, 320);
                            break;

                        case ArmedState.Miss:
                            this.ScaleTo(initial_scale * 0.8f, 320, Easing.In);
                            break;
                    }
                }
            }

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt))
            {
                centre.ScaleTo(0);
                mainContainer.ScaleTo(0);

                using (BeginDelayedSequence(spinner.TimePreempt / 2))
                {
                    centre.ScaleTo(0.3f, spinner.TimePreempt / 4, Easing.OutQuint);
                    mainContainer.ScaleTo(0.2f, spinner.TimePreempt / 4, Easing.OutQuint);

                    using (BeginDelayedSequence(spinner.TimePreempt / 2))
                    {
                        centre.ScaleTo(0.5f, spinner.TimePreempt / 2, Easing.OutQuint);
                        mainContainer.ScaleTo(1, spinner.TimePreempt / 2, Easing.OutQuint);
                    }
                }
            }

            if (drawableSpinner.Result?.TimeCompleted is double completionTime)
            {
                using (BeginAbsoluteSequence(completionTime))
                    updateDiscColour(true, 200);
            }
        }

        private void updateDiscColour(bool complete, double duration = 0)
        {
            var colour = complete ? completeColour : normalColour;

            ticks.FadeAccent(colour.Darken(1), duration);
            fill.FadeAccent(colour.Darken(1), duration);

            background.FadeAccent(colour, duration);
            centre.FadeAccent(colour, duration);
        }

        private bool checkNewRotationCount
        {
            get
            {
                int rotations = (int)(drawableSpinner.Result.TotalRotation / 360);

                if (wholeRotationCount == rotations) return false;

                wholeRotationCount = rotations;
                return true;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSpinner.IsNotNull())
                drawableSpinner.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
