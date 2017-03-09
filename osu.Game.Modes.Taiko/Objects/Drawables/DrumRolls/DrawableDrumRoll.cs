// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.DrumRoll;
using System.Linq;

namespace osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls
{
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

            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                // The body will over-shoot by CircleRadius on both sides. This is intended
                // as it means the first tick is positioned after the semi-circle.
                body = CreateBody(),
                ticks = new Container<DrawableDrumRollTick>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both
                }
            };

            body.Kiai = HitObject.Kiai;

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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.YellowDarker;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Size = new Vector2((float)(HitObject.Duration / drumRoll.PreEmpt) * (1f / Scale.X), TaikoHitObject.CIRCLE_RADIUS * 2);
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
        /// <returns>The body piece.</returns>
        protected virtual DrumRollBodyPiece CreateBody() => new DrumRollBodyPiece();

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
                return;

            if (Judgement.TimeOffset < 0)
                return;

            TaikoJudgementInfo taikoJudgement = Judgement as TaikoJudgementInfo;

            int countHit = NestedHitObjects.Count(t => t.Judgement.Result == HitResult.Hit);

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
