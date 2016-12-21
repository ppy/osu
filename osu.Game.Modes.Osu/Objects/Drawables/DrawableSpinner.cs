using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSpinner : DrawableOsuHitObject
    {
        private Spinner spinner;

        SpinnerDisc disc;
        BackSpinner backBox;

        public DrawableSpinner(Spinner s) : base(s)
        {
            spinner = s;

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
            };
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override void Update()
        {
            base.Update();
        }

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
            }
        }
        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            backBox.ScaleTo(0, spinner.Duration);

            Delay(HitObject.Duration, true);

            FadeOut(160);
        }

    }

}
