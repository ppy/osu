// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Modes.Objects.Drawables;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDonFinisher : DrawableHitCircleFinisher
    {
        public override Color4 ExplodeColour { get; protected set; }

        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.F, Key.J });

        public DrawableHitCircleDonFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.Pink;
        }

        protected override CirclePiece CreateBody() => new DonCirclePiece
        {
            Scale = new Vector2(1.5f)
        };
    }

    public class DrawableHitCircleKatsuFinisher : DrawableHitCircleFinisher
    {
        public override Color4 ExplodeColour { get; protected set; }

        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.K });

        public DrawableHitCircleKatsuFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.Blue;
        }

        protected override CirclePiece CreateBody() => new KatsuCirclePiece
        {
            Scale = new Vector2(1.5f)
        };
    }

    public abstract class DrawableHitCircleFinisher : DrawableHitCircle
    {
        private const double second_hit_window = 30;

        private List<Key> pressedKeys = new List<Key>();

        private bool validKeyPressed;

        public DrawableHitCircleFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size *= 1.5f;
        }

        protected override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo() { MaxScore = TaikoScoreResult.Great, SecondHit = true };

        protected override void CheckJudgement(bool userTriggered)
        {
            TaikoJudgementInfo tji = Judgement as TaikoJudgementInfo;

            if (!tji.Result.HasValue)
            {
                base.CheckJudgement(userTriggered);
                return;
            }

            double timeOffset = Time.Current - HitObject.EndTime;
            double hitOffset = Math.Abs(timeOffset - tji.TimeOffset);

            if (!userTriggered)
                return;

            if (!validKeyPressed)
                return;

            if (hitOffset < 30)
                tji.SecondHit = true;
        }

        protected override bool HandleKeyPress(Key key)
        {
            // Don't handle re-presses of the same key
            if (pressedKeys.Contains(key))
                return false;

            bool handled = base.HandleKeyPress(key);

            // Only add to list if this HitObject handled the keypress
            if (handled)
                pressedKeys.Add(key);

            return handled;
        }
    }
}
