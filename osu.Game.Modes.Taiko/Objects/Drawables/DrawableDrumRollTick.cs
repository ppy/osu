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

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        protected virtual List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });
        private DrumRollTick drumRollTick;

        public DrawableDrumRollTick(DrumRollTick drumRollTick)
            : base(drumRollTick)
        {
            this.drumRollTick = drumRollTick;

            RelativePositionAxes = Axes.None;

            Size = new Vector2(16) * drumRollTick.Scale;

            Masking = true;
            CornerRadius = Size.X / 2;

            Origin = Anchor.Centre;
            Anchor = Anchor.CentreLeft;

            BorderThickness = 3;
            BorderColour = Color4.White;

            Children = new[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = drumRollTick.FirstTick ? 1f : 0f,
                    AlwaysPresent = true
                }
            };

            AlwaysPresent = true;
        }

        public override JudgementInfo CreateJudgementInfo() => new TaikoDrumRollJudgementInfo() { MaxScore = TaikoScoreResult.Great };

        protected override void UpdateInitialState()
        {
        }

        protected override void UpdatePreemptState()
        {
        }

        protected override void Update()
        {
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!Keys.Contains(args.Key))
                return false;

            Keys.RemoveAll(k => k == args.Key);

            UpdateJudgement(true);
            return true;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > drumRollTick.TickDistance / 2)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            TaikoJudgementInfo taikoJudgement = Judgement as TaikoJudgementInfo;
            
            if (hitOffset < drumRollTick.TickDistance / 2)
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
                    const double flash_in = 100;

                    ScaleTo(1.5f, flash_in);
                    FadeOut(flash_in);
                    break;
            }
        }
    }
}
