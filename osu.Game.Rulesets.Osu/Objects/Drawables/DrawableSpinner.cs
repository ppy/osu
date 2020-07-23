// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinner : DrawableOsuHitObject
    {
        protected readonly Spinner Spinner;

        public readonly SpinnerDisc Disc;
        public readonly SpinnerTicks Ticks;
        public readonly SpinnerSpmCounter SpmCounter;

        private readonly Container mainContainer;

        public readonly SpinnerBackground Background;
        private readonly Container circleContainer;
        private readonly CirclePiece circle;
        private readonly GlowPiece glow;

        private readonly SpriteIcon symbol;

        private readonly Color4 baseColour = Color4Extensions.FromHex(@"002c3c");
        private readonly Color4 fillColour = Color4Extensions.FromHex(@"005b7c");

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();

        private Color4 normalColour;
        private Color4 completeColour;

        public DrawableSpinner(Spinner s)
            : base(s)
        {
            Origin = Anchor.Centre;
            Position = s.Position;

            RelativeSizeAxes = Axes.Both;

            // we are slightly bigger than our parent, to clip the top and bottom of the circle
            Height = 1.3f;

            Spinner = s;

            InternalChildren = new Drawable[]
            {
                circleContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        glow = new GlowPiece(),
                        circle = new CirclePiece
                        {
                            Position = Vector2.Zero,
                            Anchor = Anchor.Centre,
                        },
                        new RingPiece(),
                        symbol = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(48),
                            Icon = FontAwesome.Solid.Asterisk,
                            Shadow = false,
                        },
                    }
                },
                mainContainer = new AspectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        Background = new SpinnerBackground
                        {
                            Disc =
                            {
                                Alpha = 0f,
                            },
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        Disc = new SpinnerDisc(Spinner)
                        {
                            Scale = Vector2.Zero,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        circleContainer.CreateProxy(),
                        Ticks = new SpinnerTicks
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
                SpmCounter = new SpinnerSpmCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = 120,
                    Alpha = 0
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            normalColour = baseColour;
            completeColour = colours.YellowLight;

            Background.AccentColour = normalColour;
            Ticks.AccentColour = normalColour;

            Disc.AccentColour = fillColour;
            circle.Colour = colours.BlueDark;
            glow.Colour = colours.BlueDark;

            positionBindable.BindValueChanged(pos => Position = pos.NewValue);
            positionBindable.BindTo(HitObject.PositionBindable);
        }

        public float Progress => Math.Clamp(Disc.CumulativeRotation / 360 / Spinner.SpinsRequired, 0, 1);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (Progress >= 1 && !Disc.Complete)
            {
                Disc.Complete = true;
                transformFillColour(completeColour, 200);
            }

            if (userTriggered || Time.Current < Spinner.EndTime)
                return;

            ApplyResult(r =>
            {
                if (Progress >= 1)
                    r.Type = HitResult.Great;
                else if (Progress > .9)
                    r.Type = HitResult.Good;
                else if (Progress > .75)
                    r.Type = HitResult.Meh;
                else if (Time.Current >= Spinner.EndTime)
                    r.Type = HitResult.Miss;
            });
        }

        protected override void Update()
        {
            base.Update();
            if (HandleUserInput)
                Disc.Tracking = OsuActionInputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!SpmCounter.IsPresent && Disc.Tracking)
                SpmCounter.FadeIn(HitObject.TimeFadeIn);

            circle.Rotation = Disc.Rotation;
            Ticks.Rotation = Disc.Rotation;
            SpmCounter.SetRotation(Disc.CumulativeRotation);

            float relativeCircleScale = Spinner.Scale * circle.DrawHeight / mainContainer.DrawHeight;
            float targetScale = relativeCircleScale + (1 - relativeCircleScale) * Progress;
            Disc.Scale = new Vector2((float)Interpolation.Lerp(Disc.Scale.X, targetScale, Math.Clamp(Math.Abs(Time.Elapsed) / 100, 0, 1)));

            symbol.Rotation = (float)Interpolation.Lerp(symbol.Rotation, Disc.Rotation / 2, Math.Clamp(Math.Abs(Time.Elapsed) / 40, 0, 1));
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            circleContainer.ScaleTo(0);
            mainContainer.ScaleTo(0);

            using (BeginDelayedSequence(HitObject.TimePreempt / 2, true))
            {
                float phaseOneScale = Spinner.Scale * 0.7f;

                circleContainer.ScaleTo(phaseOneScale, HitObject.TimePreempt / 4, Easing.OutQuint);

                mainContainer
                    .ScaleTo(phaseOneScale * circle.DrawHeight / DrawHeight * 1.6f, HitObject.TimePreempt / 4, Easing.OutQuint)
                    .RotateTo((float)(25 * Spinner.Duration / 2000), HitObject.TimePreempt + Spinner.Duration);

                using (BeginDelayedSequence(HitObject.TimePreempt / 2, true))
                {
                    circleContainer.ScaleTo(Spinner.Scale, 400, Easing.OutQuint);
                    mainContainer.ScaleTo(1, 400, Easing.OutQuint);
                }
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            using (BeginDelayedSequence(Spinner.Duration, true))
            {
                this.FadeOut(160);

                switch (state)
                {
                    case ArmedState.Hit:
                        transformFillColour(completeColour, 0);
                        this.ScaleTo(Scale * 1.2f, 320, Easing.Out);
                        mainContainer.RotateTo(mainContainer.Rotation + 180, 320);
                        break;

                    case ArmedState.Miss:
                        this.ScaleTo(Scale * 0.8f, 320, Easing.In);
                        break;
                }
            }
        }

        private void transformFillColour(Colour4 colour, double duration)
        {
            Disc.FadeAccent(colour, duration);

            Background.FadeAccent(colour.Darken(1), duration);
            Ticks.FadeAccent(colour, duration);

            circle.FadeColour(colour, duration);
            glow.FadeColour(colour, duration);
        }
    }
}