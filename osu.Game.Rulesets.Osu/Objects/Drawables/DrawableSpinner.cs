// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinner : DrawableOsuHitObject
    {
        private readonly Spinner spinner;

        private readonly SpinnerDisc disc;
        private readonly SpinnerBackground background;
        private readonly Container circleContainer;
        private readonly DrawableHitCircle circle;

        public DrawableSpinner(Spinner s) : base(s)
        {
            AlwaysReceiveInput = true;

            Origin = Anchor.Centre;
            Position = s.Position;

            //take up full playfield.
            Size = new Vector2(OsuPlayfield.BASE_SIZE.X);

            spinner = s;

            Children = new Drawable[]
            {
                background = new SpinnerBackground
                {
                    Alpha = 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DiscColour = Color4.Black
                },
                disc = new SpinnerDisc
                {
                    Alpha = 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DiscColour = AccentColour
                },
                circleContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new []
                    {
                        circle = new DrawableHitCircle(s)
                        {
                            Interactive = false,
                            Position = Vector2.Zero,
                            Anchor = Anchor.Centre,
                        }
                    }
                }
            };

            background.Scale = scaleToCircle;
            disc.Scale = scaleToCircle;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (Time.Current < HitObject.StartTime) return;

            disc.ScaleTo(Interpolation.ValueAt(Math.Sqrt(Progress), scaleToCircle, Vector2.One, 0, 1), 100);

            if (Progress >= 1)
                disc.Complete = true;

            if (!userTriggered && Time.Current >= spinner.EndTime)
            {
                if (Progress >= 1)
                {
                    Judgement.Score = OsuScoreResult.Hit300;
                    Judgement.Result = HitResult.Hit;
                }
                else if (Progress > .9)
                {
                    Judgement.Score = OsuScoreResult.Hit100;
                    Judgement.Result = HitResult.Hit;
                }
                else if (Progress > .75)
                {
                    Judgement.Score = OsuScoreResult.Hit50;
                    Judgement.Result = HitResult.Hit;
                }
                else
                {
                    Judgement.Score = OsuScoreResult.Miss;
                    if (Time.Current >= spinner.EndTime)
                        Judgement.Result = HitResult.Miss;
                }
            }
        }

        private Vector2 scaleToCircle => circle.Scale * circle.DrawWidth / DrawWidth * 0.95f;

        private const float spins_per_minute_needed = 100 + 5 * 15; //TODO: read per-map OD and place it on the 5

        private float rotationsNeeded => (float)(spins_per_minute_needed * (spinner.EndTime - spinner.StartTime) / 60000f);

        public float Progress => MathHelper.Clamp(disc.RotationAbsolute / 360 / rotationsNeeded, 0, 1);

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            circleContainer.ScaleTo(1, 400, EasingTypes.OutElastic);

            background.Delay(TIME_PREEMPT - 500);

            background.ScaleTo(scaleToCircle * 1.2f, 400, EasingTypes.OutQuint);
            background.FadeIn(200);

            background.Delay(400);
            background.ScaleTo(1, 250, EasingTypes.OutQuint);

            disc.Delay(TIME_PREEMPT - 50);
            disc.FadeIn(200);
        }

        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            Delay(spinner.Duration, true);

            FadeOut(160);

            switch (state)
            {
                case ArmedState.Hit:
                    ScaleTo(Scale * 1.2f, 320, EasingTypes.Out);
                    Expire();
                    break;
                case ArmedState.Miss:
                    ScaleTo(Scale * 0.8f, 320, EasingTypes.In);
                    Expire();
                    break;
            }
        }
    }
}
