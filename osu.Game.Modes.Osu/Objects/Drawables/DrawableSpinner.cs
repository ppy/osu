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

        private SpinnerDisc disc;
        private BackSpinner backBox;
        private CirclePiece circle;
        private GlowPiece circleGlow;
        private NumberPiece number;
        private ExplodePiece explode;
        private RingPiece ring;
        private Container contCircle;

        public DrawableSpinner(Spinner s) : base(s)
        {
            spinner = s;
            Position = s.Position;

            Children = new Drawable[]
            {
                backBox = new BackSpinner(spinner)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                disc = new SpinnerDisc(spinner)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
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
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
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
                explode = new ExplodePiece
                {
                    Scale = new Vector2(s.Scale),
                    Colour = s.Colour,
                },
            };
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override void Update()
        {
            base.Update();
        }
        private OsuJudgementInfo j;

        protected override void CheckJudgement(bool userTriggered)
        {
            var j = Judgement as OsuJudgementInfo;

            if (!userTriggered && Time.Current >= HitObject.EndTime)
            {
                if (disc.Progress == 1)
                {
                    j.Score = OsuScoreResult.Hit300;
                    j.Result = HitResult.Hit;
                }
                else if (disc.Progress > .9)
                {
                    j.Score = OsuScoreResult.Hit100;
                    j.Result = HitResult.Hit;
                }
                else if (disc.Progress >.75)
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

        /*protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

        }*/
        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            backBox.ScaleTo(0, spinner.Duration);

            Delay(HitObject.Duration, true);

            disc.FadeOut(160);
            backBox.FadeOut(160);
            contCircle.FadeOut(160);
            explode.FadeIn(40);
            Delay(40, true);
            FadeOut(800);
            explode.ScaleTo(spinner.Scale * 1.5f, 400, EasingTypes.OutQuad);
        }

    }

}
