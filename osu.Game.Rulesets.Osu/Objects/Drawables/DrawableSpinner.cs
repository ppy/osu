// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Screens.Ranking;
using osu.Game.Rulesets.Scoring;

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

        private Color4 normalColour;
        private Color4 completeColour;

        public DrawableSpinner(Spinner s) : base(s)
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
                            Icon = FontAwesome.fa_asterisk,
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

        public float Progress => MathHelper.Clamp(Disc.RotationAbsolute / 360 / Spinner.SpinsRequired, 0, 1);

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
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

            if (!userTriggered && Time.Current >= Spinner.EndTime)
            {
                if (Progress >= 1)
                    AddJudgement(new OsuJudgement { Result = HitResult.Great });
                else if (Progress > .9)
                    AddJudgement(new OsuJudgement { Result = HitResult.Good });
                else if (Progress > .75)
                    AddJudgement(new OsuJudgement { Result = HitResult.Meh });
                else if (Time.Current >= Spinner.EndTime)
                    AddJudgement(new OsuJudgement { Result = HitResult.Miss });
            }
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
        }

        protected override void Update()
        {
            Disc.Tracking = OsuActionInputManager.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton);
            if (!spmCounter.IsPresent && Disc.Tracking)
                spmCounter.FadeIn(HitObject.TimeFadein);

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

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

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

        protected override void UpdateCurrentState(ArmedState state)
        {
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

            Expire();
        }
    }
}
