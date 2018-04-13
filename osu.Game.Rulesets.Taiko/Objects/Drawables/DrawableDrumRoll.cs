﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRoll : DrawableTaikoHitObject<DrumRoll>
    {
        /// <summary>
        /// Number of rolling hits required to reach the dark/final colour.
        /// </summary>
        private const int rolling_hits_for_engaged_colour = 5;

        /// <summary>
        /// Rolling number of tick hits. This increases for hits and decreases for misses.
        /// </summary>
        private int rollingHits;

        public DrawableDrumRoll(DrumRoll drumRoll)
            : base(drumRoll)
        {
            RelativeSizeAxes = Axes.Y;

            Container<DrawableDrumRollTick> tickContainer;
            MainPiece.Add(tickContainer = new Container<DrawableDrumRollTick> { RelativeSizeAxes = Axes.Both });

            foreach (var tick in drumRoll.NestedHitObjects.OfType<DrumRollTick>())
            {
                var newTick = new DrawableDrumRollTick(tick);
                newTick.OnJudgement += onTickJudgement;

                AddNested(newTick);
                tickContainer.Add(newTick);
            }
        }

        protected override TaikoPiece CreateMainPiece() => new ElongatedCirclePiece();

        public override bool OnPressed(TaikoAction action) => false;

        private Color4 colourIdle;
        private Color4 colourEngaged;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            MainPiece.AccentColour = colourIdle = colours.YellowDark;
            colourEngaged = colours.YellowDarker;
        }

        private void onTickJudgement(DrawableHitObject obj, Judgement judgement)
        {
            if (judgement.Result > HitResult.Miss)
                rollingHits++;
            else
                rollingHits--;

            rollingHits = MathHelper.Clamp(rollingHits, 0, rolling_hits_for_engaged_colour);

            Color4 newColour = Interpolation.ValueAt((float)rollingHits / rolling_hits_for_engaged_colour, colourIdle, colourEngaged, 0, 1);
            MainPiece.FadeAccent(newColour, 100);
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (userTriggered)
                return;

            if (timeOffset < 0)
                return;

            int countHit = NestedHitObjects.Count(o => o.IsHit);
            if (countHit >= HitObject.RequiredGoodHits)
            {
                AddJudgement(new TaikoJudgement { Result = countHit >= HitObject.RequiredGreatHits ? HitResult.Great : HitResult.Good });
                if (HitObject.IsStrong)
                    AddJudgement(new TaikoStrongHitJudgement());
            }
            else
                AddJudgement(new TaikoJudgement { Result = HitResult.Miss });
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                case ArmedState.Miss:
                    this.FadeOut(100).Expire();
                    break;
            }
        }
    }
}
