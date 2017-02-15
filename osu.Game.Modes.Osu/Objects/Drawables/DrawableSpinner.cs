// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableSpinner : DrawableOsuHitObject
    {
        private Spinner spinner;

        private SpinnerDisc disc;
        private SpinnerBackground background;
        private Container circleContainer;
        private DrawableHitCircle circle;

        public DrawableSpinner(Spinner s) : base(s)
        {
            Origin = Anchor.Centre;
            Position = s.Position;

            //take up full playfield.
            Size = new Vector2(512);

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
                    DiscColour = s.Colour
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

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override void CheckJudgement(bool userTriggered)
        {
            if (Time.Current < HitObject.StartTime) return;

            var j = Judgement as OsuJudgementInfo;

            disc.ScaleTo(Interpolation.ValueAt(Math.Sqrt(Progress), scaleToCircle, Vector2.One, 0, 1), 100);

            if (Progress >= 1)
                disc.Complete = true;

            if (!userTriggered && Time.Current >= HitObject.EndTime)
            {
                if (Progress >= 1)
                {
                    j.Score = OsuScoreResult.Hit300;
                    j.Result = HitResult.Hit;
                }
                else if (Progress > .9)
                {
                    j.Score = OsuScoreResult.Hit100;
                    j.Result = HitResult.Hit;
                }
                else if (Progress > .75)
                {
                    j.Score = OsuScoreResult.Hit50;
                    j.Result = HitResult.Hit;
                }
                else
                {
                    j.Score = OsuScoreResult.Miss;
                    if (Time.Current >= HitObject.EndTime)
                        j.Result = HitResult.Miss;
                }
            }
        }

        private Vector2 scaleToCircle => new Vector2(circle.Scale * circle.DrawWidth / DrawWidth) * 0.95f;

        private float spinsPerMinuteNeeded = 100 + (5 * 15); //TODO: read per-map OD and place it on the 5

        private float rotationsNeeded => (float)(spinsPerMinuteNeeded * (spinner.EndTime - spinner.StartTime) / 60000f);

        public float Progress => MathHelper.Clamp(disc.RotationAbsolute / 360 / rotationsNeeded, 0, 1);

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            FadeIn(200);
            circleContainer.ScaleTo(1, 400, EasingTypes.OutElastic);

            background.Delay(TIME_PREEMPT - 100);
            background.FadeIn(200);
            background.ScaleTo(1, 200, EasingTypes.OutQuint);

            disc.Delay(TIME_PREEMPT - 50);
            disc.FadeIn(200);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            base.UpdateState(state);

            Delay(HitObject.Duration, true);

            FadeOut(160);

            switch (state)
            {
                case ArmedState.Hit:
                    ScaleTo(Scale * 1.2f, 320, EasingTypes.Out);
                    break;
                case ArmedState.Miss:
                    ScaleTo(Scale * 0.8f, 320, EasingTypes.In);
                    break;
            }
        }
    }
}
