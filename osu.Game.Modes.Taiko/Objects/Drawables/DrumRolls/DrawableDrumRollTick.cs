// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        public override Color4 ExplodeColour { get; protected set; }

        protected virtual List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private Container bodyPiece;

        private DrumRollTick drumRollTick;

        public DrawableDrumRollTick(DrumRollTick drumRollTick)
            : base(drumRollTick)
        {
            this.drumRollTick = drumRollTick;

            RelativePositionAxes = Axes.X;

            Size = new Vector2(24);

            Origin = Anchor.Centre;
            Anchor = Anchor.CentreLeft;

            Children = new Drawable[]
            {
                bodyPiece = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Masking = true,
                    CornerRadius = Size.X / 2,

                    BorderThickness = 6,
                    BorderColour = Color4.White,

                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Alpha = drumRollTick.FirstTick ? 1f : 0f,
                            AlwaysPresent = true
                        }
                    }
                }
            };

            ExplodeColour = Color4.White;
        }

        protected override JudgementInfo CreateJudgementInfo() => new TaikoDrumRollTickJudgementInfo { MaxScore = TaikoScoreResult.Great };

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

            TaikoJudgementInfo taikoJudgement = (TaikoJudgementInfo)Judgement;

            if (hitOffset < drumRollTick.TickTimeDistance / 2)
            {
                taikoJudgement.Result = HitResult.Hit;
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
                    bodyPiece.ScaleTo(0, 100, EasingTypes.OutQuint);
                    break;
            }
        }

        protected override void Update()
        {
            // Drum roll ticks don't move
        }
    }
}
