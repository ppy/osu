// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using osu.Game.Modes.Taiko.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableDrumRollFinisher : DrawableDrumRoll
    {
        public DrawableDrumRollFinisher(DrumRoll drumRoll)
            : base(drumRoll)
        {
            Size *= new Vector2(1, 1.5f);
        }

        protected override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo() { MaxScore = TaikoScoreResult.Great, SecondHit = true };

        protected override DrawableDrumRollTick CreateTick(DrumRoll drumRoll, DrumRollTick tick) => new DrawableDrumRollFinisherTick(drumRoll, tick);

        protected override DrumRollBodyPiece CreateBody(float length) => new DrumRollFinisherBodyPiece(length);
    }

    public class DrawableDrumRoll : DrawableTaikoHitObject
    {
        public override Color4 ExplodeColour { get; protected set; }

        private DrumRoll drumRoll;

        private DrumRollBodyPiece body;
        private Container<DrawableDrumRollTick> ticks;

        public DrawableDrumRoll(DrumRoll drumRoll)
            : base(drumRoll)
        {
            this.drumRoll = drumRoll;

            Origin = Anchor.CentreLeft;

            Size = new Vector2(1, TaikoHitObject.CIRCLE_RADIUS * 2);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.YellowDarker;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Size = new Vector2((float)(HitObject.Duration / drumRoll.PreEmpt) * Parent.DrawSize.X * (1f / Scale.X), Size.Y);

            Children = new Drawable[]
            {
                // The body will over-shoot by CircleRadius on both sides. This is intended
                // as it means the first tick is positioned after the semi-circle.
                body = CreateBody(Size.X),
                ticks = new Container<DrawableDrumRollTick>()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both
                }
           };

            int tickIndex = 0;
            foreach (var tick in drumRoll.Ticks)
            {
                var newTick = CreateTick(drumRoll, tick);
                newTick.Depth = tickIndex;
                newTick.Position = new Vector2((float)((tick.StartTime - HitObject.StartTime) / HitObject.Duration), 0);

                ticks.Add(newTick);
                AddNested(newTick);

                tickIndex++;
            }
        }
        
        /// <summary>
        /// Creates a drawable tick from the base tick.
        /// </summary>
        /// <param name="drumRoll">The drum roll that contains the tick.</param>
        /// <param name="tick">The base tick.</param>
        /// <returns>The drawable tick.</returns>
        protected virtual DrawableDrumRollTick CreateTick(DrumRoll drumRoll, DrumRollTick tick) => new DrawableDrumRollTick(drumRoll, tick);

        /// <summary>
        /// Creates the body piece of the drum roll.
        /// </summary>
        /// <param name="length">The raw length of the body piece, before CornerRadius.</param>
        /// <returns>The body piece.</returns>
        protected virtual DrumRollBodyPiece CreateBody(float length) => new DrumRollBodyPiece(length);

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
                return;

            if (Judgement.TimeOffset < 0)
                return;

            TaikoJudgementInfo taikoJudgement = Judgement as TaikoJudgementInfo;

            int countHit = NestedHitObjects.Count(t => t.Judgement.Result.HasValue);

            if (countHit > drumRoll.RequiredGoodHits)
            {
                Judgement.Result = HitResult.Hit;

                if (countHit >= drumRoll.RequiredGreatHits)
                    taikoJudgement.Score = TaikoScoreResult.Great;
                else
                    taikoJudgement.Score = TaikoScoreResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }
    }
}
