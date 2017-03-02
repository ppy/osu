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
    public class DrawableDrumRollTick : DrawableTaikoHitObject, IDrawableHitObjectWithProxiedApproach
    {
        public Drawable ProxiedLayer { get; set; }

        protected virtual List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private Container bodyPiece;
        private ExplodePiece explodePiece;

        private DrumRoll drumRoll;
        private DrumRollTick drumRollTick;

        private double hitDuration => drumRoll.TickDistance / drumRoll.Length * drumRoll.Duration;

        public DrawableDrumRollTick(DrumRoll drumRoll, DrumRollTick drumRollTick)
            : base(drumRollTick)
        {
            this.drumRoll = drumRoll;
            this.drumRollTick = drumRollTick;

            RelativePositionAxes = Axes.None;

            Size = new Vector2(16) * drumRollTick.Scale;

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
                        },
                    }
                },
                ProxiedLayer = new Container()
                {
                    RelativeSizeAxes = Axes.Both,

                    Children = new[]
                    {
                        explodePiece = new ExplodePiece()
                        {
                            RelativeSizeAxes = Axes.None,
                            Size = new Vector2(128),

                            Colour = new Color4(238, 170, 0, 255),
                        }
                    }
                }
            };

            AlwaysPresent = true;
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
                if (Judgement.TimeOffset > hitDuration / 2)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            TaikoJudgementInfo taikoJudgement = Judgement as TaikoJudgementInfo;
            
            if (hitOffset < hitDuration / 2)
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
                    const double flash_in = 200;

                    explodePiece.FadeIn();
                    explodePiece.ScaleTo(3f, flash_in);
                    explodePiece.FadeOut(flash_in);

                    bodyPiece.ScaleTo(1.5f, flash_in);
                    bodyPiece.FadeOut(flash_in);

                    Delay(flash_in * 2);
                    break;
            }
        }

        protected override void Update()
        {
            if (State == ArmedState.Hit)
                MoveToOffset(Time.Current - HitObject.StartTime + Judgement.TimeOffset);
        }
    }
}
