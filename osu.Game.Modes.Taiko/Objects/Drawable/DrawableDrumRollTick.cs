// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System.Collections.Generic;
using osu.Game.Modes.Taiko.Judgements;
using System;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;
using osu.Framework.Graphics.Containers;
using OpenTK.Graphics;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        /// <summary>
        /// A list of keys which this HitObject will accept. These are the standard Taiko keys for now.
        /// These should be moved to bindings later.
        /// </summary>
        private List<Key> validKeys = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private DrumRollTick tick;

        private CircularContainer tickContainer;

        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
            this.tick = tick;

            Children = new[]
            {
                tickContainer = new CircularContainer
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.CentreLeft,

                    Size = new Vector2(26),

                    BorderThickness = 5,
                    BorderColour = Color4.White,

                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,

                            Alpha = tick.FirstTick ? 1 : 0,
                            AlwaysPresent = true
                        }
                    }
                }
            };

            LifetimeStart = double.MinValue;
            LifetimeEnd = double.MaxValue;
        }

        protected override TaikoJudgementInfo CreateJudgementInfo() => new TaikoDrumRollTickJudgementInfo();

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > tick.TickTimeDistance / 2)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            if (Math.Abs(Judgement.TimeOffset) < tick.TickTimeDistance / 2)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.Score = TaikoScoreResult.Great;
            }
        }

        protected override void Update()
        {
            // Drum roll ticks shouldn't move
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat)
                return false;

            if (Judgement.Result.HasValue)
                return false;

            if (!validKeys.Contains(args.Key))
                return false;

            return UpdateJudgement(true);
        }

        /// <summary>
        /// Empty piece (ticks don't use this).
        /// </summary>
        protected override ScrollingCirclePiece CreateCircle() => new ScrollingCirclePiece();
    }
}
