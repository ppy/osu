using osu.Framework.Graphics;
using OpenTK.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSpinner : DrawableOsuHitObject
    {
        private Spinner spinner;

        private SpinnerDisc disc; //Change to SpinnerTrigger
        private CirclePiece circle;
        private GlowPiece circleGlow;
        private NumberPiece number;
        private ExplodePiece explode;
        private RingPiece ring;
        private Container contCircle;
        private SpinnerBottom bottom;
        private SpinnerMiddle middle;
        private SpinnerCursorTrail follow;
        private SpinnerProgress progress;

        private const float scaleSpinner = 1;

        public DrawableSpinner(Spinner s) : base(s)
        {
            spinner = s;
            Position = s.Position;

            Children = new Drawable[]
            {
                explode = new ExplodePiece
                {
                    Scale = new Vector2(s.Scale),
                    Colour = s.Colour,
                },
                bottom = new SpinnerBottom(spinner)
                {
                    Scale = new Vector2(s.Scale)
                },
                middle = new SpinnerMiddle(spinner)
                {
                    Scale = new Vector2(s.Scale)
                },
                progress = new SpinnerProgress(spinner)
                {
                    Scale = new Vector2(s.Scale)
                },
                follow = new SpinnerCursorTrail(spinner),
                disc = new SpinnerDisc(spinner),
                contCircle = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        circleGlow = new GlowPiece
                        {
                            Scale = new Vector2(s.Scale),
                            Colour = s.Colour,
                        },
                        circle = new CirclePiece
                        {
                            //Position = s.Position,
                            Scale = new Vector2(s.Scale),
                            Colour = s.Colour,
                        },
                        number = new NumberPiece
                        {
                            Scale = new Vector2(s.Scale)
                        },
                        ring = new RingPiece
                        {
                            Scale = new Vector2(s.Scale),
                        },

                    }
                },
            };
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override void Update()
        {
            
            base.Update();
            progress.Progress = disc.SpinProgress;
            progress.IsSpinningLeft = disc.IsSpinningLeft;
            follow.MousePosition = disc.DistanceToCentre;
            follow.MouseAngle = disc.ActualAngle;
            follow.Tracking = disc.Tracking;
        }
        private OsuJudgementInfo j;

        protected override void CheckJudgement(bool userTriggered)
        {
            var j = Judgement as OsuJudgementInfo;

            if (!userTriggered && Time.Current >= HitObject.EndTime)
            {
                if (disc.SpinProgress == 1)
                {
                    j.Score = OsuScoreResult.Hit300;
                    j.Result = HitResult.Hit;
                }
                else if (disc.SpinProgress > .9)
                {
                    j.Score = OsuScoreResult.Hit100;
                    j.Result = HitResult.Hit;
                }
                else if (disc.SpinProgress >.75)
                {
                    j.Score = OsuScoreResult.Hit50;
                    j.Result = HitResult.Hit;
                }
                else
                {
                    j.Score = OsuScoreResult.Miss;
                    j.Result = HitResult.Miss;
                }

                this.j = j;
            }
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();
            

            bottom.ScaleTo(scaleSpinner * 2f, TIME_PREEMPT, EasingTypes.OutQuad);
            middle.ScaleTo(scaleSpinner * 1.65f, TIME_PREEMPT, EasingTypes.OutSine);
            progress.ScaleTo(scaleSpinner * 1.65f);
            Delay(60);
            progress.FadeTo(1, TIME_PREEMPT - 100);


        }
        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            base.UpdateState(state);

            Delay(HitObject.Duration, true);

            disc.FadeOut(160);
            bottom.FadeOut(160);
            middle.FadeOut(160);
            contCircle.FadeOut(160);
            follow.FadeOut(160);
            progress.FadeOut(160);
            explode.FadeIn(40);
            Delay(40, true);
            FadeOut(800);
            explode.ScaleTo(spinner.Scale * 1.5f, 400, EasingTypes.OutQuad);
        }

    }

}
