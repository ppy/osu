// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonSpinnerDisc : CompositeDrawable
    {
        private const float initial_scale = 1f;
        private const float idle_alpha = 0.2f;
        private const float tracking_alpha = 0.4f;

        private const float idle_centre_size = 80f;
        private const float tracking_centre_size = 40f;

        private DrawableSpinner drawableSpinner = null!;

        private readonly BindableBool complete = new BindableBool();

        private int wholeRotationCount;

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

        private Container disc = null!;
        private Container centre = null!;
        private CircularContainer fill = null!;

        private Container ticksContainer = null!;
        private ArgonSpinnerTicks ticks = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            drawableSpinner = (DrawableSpinner)drawableHitObject;

            // we are slightly bigger than our parent, to clip the top and bottom of the circle
            // this should probably be revisited when scaled spinners are a thing.
            Scale = new Vector2(initial_scale);

            InternalChildren = new Drawable[]
            {
                disc = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(8f),
                            Children = new[]
                            {
                                fill = new CircularContainer
                                {
                                    Name = @"Fill",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Colour = Colour4.FromHex("FC618F").Opacity(1f),
                                        Radius = 40,
                                    },
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0f,
                                        AlwaysPresent = true,
                                    }
                                },
                                ticksContainer = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Child = ticks = new ArgonSpinnerTicks(),
                                }
                            },
                        },
                        new Container
                        {
                            Name = @"Ring",
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(8f),
                            Children = new[]
                            {
                                new ArgonSpinnerRingArc
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Name = "Top Arc",
                                },
                                new ArgonSpinnerRingArc
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Name = "Bottom Arc",
                                    Scale = new Vector2(1, -1),
                                },
                            }
                        },
                        new Container
                        {
                            Name = @"Sides",
                            RelativeSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                new ArgonSpinnerProgressArc
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Name = "Left Bar"
                                },
                                new ArgonSpinnerProgressArc
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Name = "Right Bar",
                                    Scale = new Vector2(-1, 1),
                                },
                            }
                        },
                    }
                },
                centre = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(idle_centre_size),
                    Children = new[]
                    {
                        new RingPiece(10)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.8f),
                        },
                        new RingPiece(3)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1f),
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableSpinner.ApplyCustomUpdateState += updateStateTransforms;

            updateStateTransforms(drawableSpinner, drawableSpinner.State.Value);
        }

        private float trackingElementInterpolation;

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
                trackingElementInterpolation =
                    (float)Interpolation.Damp(trackingElementInterpolation, drawableSpinner.RotationTracker.Tracking ? 1 : 0, 0.985f, (float)Math.Abs(Clock.ElapsedFrameTime));

                fill.Alpha = trackingElementInterpolation * (tracking_alpha - idle_alpha) + idle_alpha;
                centre.Size = new Vector2(trackingElementInterpolation * (tracking_centre_size - idle_centre_size) + idle_centre_size);
            }

            const float initial_fill_scale = 0.1f;
            float targetScale = initial_fill_scale + (0.98f - initial_fill_scale) * drawableSpinner.Progress;

            fill.Scale = new Vector2((float)Interpolation.Lerp(fill.Scale.X, targetScale, Math.Clamp(Math.Abs(Time.Elapsed) / 100, 0, 1)));
            ticks.Rotation = drawableSpinner.RotationTracker.Rotation;
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            if (!(drawableHitObject is DrawableSpinner))
                return;

            Spinner spinner = drawableSpinner.HitObject;

            using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt))
            {
                this.ScaleTo(initial_scale);
                ticksContainer.RotateTo(0);
                centre.ScaleTo(0);
                disc.ScaleTo(0);

                using (BeginDelayedSequence(spinner.TimePreempt / 2))
                {
                    // constant ambient rotation to give the spinner "spinning" character.
                    ticksContainer.RotateTo((float)(25 * spinner.Duration / 2000), spinner.TimePreempt + spinner.Duration);
                }

                using (BeginDelayedSequence(spinner.TimePreempt / 2))
                {
                    centre.ScaleTo(0.3f, spinner.TimePreempt / 4, Easing.OutQuint);
                    disc.ScaleTo(0.2f, spinner.TimePreempt / 4, Easing.OutQuint);

                    using (BeginDelayedSequence(spinner.TimePreempt / 2))
                    {
                        centre.ScaleTo(0.8f, spinner.TimePreempt / 2, Easing.OutQuint);
                        disc.ScaleTo(1, spinner.TimePreempt / 2, Easing.OutQuint);
                    }
                }

                using (BeginDelayedSequence(spinner.TimePreempt + spinner.Duration + drawableHitObject.Result.TimeOffset))
                {
                    switch (state)
                    {
                        case ArmedState.Hit:
                            disc.ScaleTo(initial_scale * 1.2f, 320, Easing.Out);
                            ticksContainer.RotateTo(ticksContainer.Rotation + 180, 320);
                            break;

                        case ArmedState.Miss:
                            disc.ScaleTo(initial_scale * 0.8f, 320, Easing.In);
                            break;
                    }
                }
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
