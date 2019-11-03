// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinner : DrawableOsuHitObject
    {
        protected readonly Spinner Spinner;

        public readonly SpinnerDisc Disc;
        public readonly SpinnerTicks Ticks;
        private readonly SpinnerSpmCounter spmCounter;

        private readonly Container mainContainer;

        public readonly SpinnerBackground Background;
        private readonly Container circleContainer;
        private readonly CirclePiece circle;
        private readonly GlowPiece glow;

        private readonly SpriteIcon symbol;

        private readonly Color4 baseColour = OsuColour.FromHex(@"002c3c");
        private readonly Color4 fillColour = OsuColour.FromHex(@"005b7c");

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
                            Alpha = 0.6f,
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
                spmCounter = new SpinnerSpmCounter
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

            Background.AccentColour = normalColour;

            completeColour = colours.YellowLight.Opacity(0.75f);

            Disc.AccentColour = fillColour;
            circle.Colour = colours.BlueDark;
            glow.Colour = colours.BlueDark;

            positionBindable.BindValueChanged(pos => Position = pos.NewValue);
            positionBindable.BindTo(HitObject.PositionBindable);
        }

        public float Progress => MathHelper.Clamp(Disc.RotationAbsolute / 360 / Spinner.SpinsRequired, 0, 1);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (Progress >= 1 && !Disc.Complete)
            {
                Disc.Complete = true;

                const float duration = 200;

                Disc.FadeAccent(completeColour, duration);

                Background.FadeAccent(completeColour, duration);
                Background.FadeOut(duration);

                circle.FadeColour(completeColour, duration);
                glow.FadeColour(completeColour, duration);
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
            Disc.Tracking = OsuActionInputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false;
            if (!spmCounter.IsPresent && Disc.Tracking)
                spmCounter.FadeIn(HitObject.TimeFadeIn);

            base.Update();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            circle.Rotation = Disc.Rotation;
            Ticks.Rotation = Disc.Rotation;
            spmCounter.SetRotation(Disc.RotationAbsolute);

            float relativeCircleScale = Spinner.Scale * circle.DrawHeight / mainContainer.DrawHeight;
            Disc.ScaleTo(relativeCircleScale + (1 - relativeCircleScale) * Progress, 200, Easing.OutQuint);

            symbol.RotateTo(Disc.Rotation / 2, 500, Easing.OutQuint);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            circleContainer.ScaleTo(Spinner.Scale * 0.3f);
            circleContainer.ScaleTo(Spinner.Scale, HitObject.TimePreempt / 1.4f, Easing.OutQuint);

            Disc.RotateTo(-720);
            symbol.RotateTo(-720);

            mainContainer
                .ScaleTo(0)
                .ScaleTo(Spinner.Scale * circle.DrawHeight / DrawHeight * 1.4f, HitObject.TimePreempt - 150, Easing.OutQuint)
                .Then()
                .ScaleTo(1, 500, Easing.OutQuint);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            var sequence = this.Delay(Spinner.Duration).FadeOut(160);

            switch (state)
            {
                case ArmedState.Hit:
                    sequence.ScaleTo(Scale * 1.2f, 320, Easing.Out);
                    break;

                case ArmedState.Miss:
                    sequence.ScaleTo(Scale * 0.8f, 320, Easing.In);
                    break;
            }
        }
    }
}
