// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinner : DrawableOsuHitObject
    {
        private readonly Spinner spinner;

        private readonly SpinnerDisc disc;
        private readonly SpinnerTicks ticks;

        private readonly Container mainContainer;

        private readonly SpinnerBackground background;
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

            spinner = s;

            Children = new Drawable[]
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
                    Children = new Drawable[]
                    {
                        background = new SpinnerBackground
                        {
                            Alpha = 0.6f,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        disc = new SpinnerDisc(spinner)
                        {
                            Scale = Vector2.Zero,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        circleContainer.CreateProxy(),
                        ticks = new SpinnerTicks
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
            };
        }

        public float Progress => MathHelper.Clamp(disc.RotationAbsolute / 360 / spinner.SpinsRequired, 0, 1);

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (Progress >= 1 && !disc.Complete)
            {
                disc.Complete = true;

                const float duration = 200;

                disc.FadeAccent(completeColour, duration);

                background.FadeAccent(completeColour, duration);
                background.FadeOut(duration);

                circle.FadeColour(completeColour, duration);
                glow.FadeColour(completeColour, duration);
            }

            if (!userTriggered && Time.Current >= spinner.EndTime)
            {
                if (Progress >= 1)
                    AddJudgement(new OsuJudgement { Result = HitResult.Great });
                else if (Progress > .9)
                    AddJudgement(new OsuJudgement { Result = HitResult.Good });
                else if (Progress > .75)
                    AddJudgement(new OsuJudgement { Result = HitResult.Meh });
                else if (Time.Current >= spinner.EndTime)
                    AddJudgement(new OsuJudgement { Result = HitResult.Miss });
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            normalColour = baseColour;

            background.AccentColour = normalColour;

            completeColour = colours.YellowLight.Opacity(0.75f);

            disc.AccentColour = fillColour;
            circle.Colour = colours.BlueDark;
            glow.Colour = colours.BlueDark;
        }

        protected override void Update()
        {
            disc.Tracking = OsuActionInputManager.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton);

            base.Update();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            circle.Rotation = disc.Rotation;
            ticks.Rotation = disc.Rotation;

            float relativeCircleScale = spinner.Scale * circle.DrawHeight / mainContainer.DrawHeight;
            disc.ScaleTo(relativeCircleScale + (1 - relativeCircleScale) * Progress, 200, Easing.OutQuint);

            symbol.RotateTo(disc.Rotation / 2, 500, Easing.OutQuint);
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            circleContainer.ScaleTo(spinner.Scale * 0.3f);
            circleContainer.ScaleTo(spinner.Scale, TIME_PREEMPT / 1.4f, Easing.OutQuint);

            disc.RotateTo(-720);
            symbol.RotateTo(-720);

            mainContainer
                .ScaleTo(0)
                .ScaleTo(spinner.Scale * circle.DrawHeight / DrawHeight * 1.4f, TIME_PREEMPT - 150, Easing.OutQuint)
                .Then()
                .ScaleTo(1, 500, Easing.OutQuint);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            var sequence = this.Delay(spinner.Duration).FadeOut(160);

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
