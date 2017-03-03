using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        public override Color4 ExplodeColour => new Color4(238, 170, 0, 255);

        protected virtual List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private Container bodyPiece;

        private DrumRoll drumRoll;
        private DrumRollTick drumRollTick;

        public DrawableDrumRollTick(DrumRoll drumRoll, DrumRollTick drumRollTick)
            : base(drumRollTick)
        {
            this.drumRoll = drumRoll;
            this.drumRollTick = drumRollTick;

            RelativePositionAxes = Axes.X;

            Size = new Vector2(12) * drumRollTick.Scale;

            Origin = Anchor.Centre;
            Anchor = Anchor.CentreLeft;

            Children = new Drawable[]
            {
                bodyPiece = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Masking = true,
                    CornerRadius = Size.X / 2,

                    BorderThickness = 3,
                    BorderColour = Color4.White,

                    Children = new[]
                    {
                        new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Alpha = drumRollTick.FirstTick ? 1f : 0f,
                            AlwaysPresent = true
                        }
                    }
                }
            };
        }

        public override JudgementInfo CreateJudgementInfo() => new TaikoDrumRollJudgementInfo() { MaxScore = TaikoScoreResult.Great };

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (Judgement.Result.HasValue)
                return false;

            if (!Keys.Contains(args.Key))
                return false;

            return UpdateJudgement(true);
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > drumRollTick.TickTimeDistance / 2)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            TaikoJudgementInfo taikoJudgement = Judgement as TaikoJudgementInfo;
            
            if (hitOffset < drumRollTick.TickTimeDistance / 2)
            {
                Judgement.Result = HitResult.Hit;
                taikoJudgement.Score = TaikoScoreResult.Great;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            base.UpdateState(state);

            switch (State)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    break;
                case ArmedState.Hit:
                    const double scale_out = 50;

                    bodyPiece.ScaleTo(0, scale_out);

                    break;
            }
        }

        protected override void Update()
        {
            base.UpdateAuto();
        }
    }
}
