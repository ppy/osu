﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield
    {
        public const float BASE_WIDTH = 512;

        private readonly CatcherArea catcherArea;

        protected override bool UserScrollSpeedAdjustment => false;

        protected override SpeedChangeVisualisationMethod VisualisationMethod => SpeedChangeVisualisationMethod.Constant;

        public CatchPlayfield(BeatmapDifficulty difficulty, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> getVisualRepresentation)
        {
            Direction.Value = ScrollingDirection.Down;

            Container explodingFruitContainer;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            Size = new Vector2(0.86f); // matches stable's vertical offset for catcher plate

            InternalChild = new PlayfieldAdjustmentContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    explodingFruitContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    catcherArea = new CatcherArea(difficulty)
                    {
                        GetVisualRepresentation = getVisualRepresentation,
                        ExplodingFruitTarget = explodingFruitContainer,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.TopLeft,
                    },
                    HitObjectContainer
                }
            };

            VisibleTimeRange.Value = BeatmapDifficulty.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450);
        }

        public bool CheckIfWeCanCatch(CatchHitObject obj) => catcherArea.AttemptCatch(obj);

        public override void Add(DrawableHitObject h)
        {
            h.OnNewResult += onNewResult;

            base.Add(h);

            var fruit = (DrawableCatchHitObject)h;
            fruit.CheckPosition = CheckIfWeCanCatch;
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
            => catcherArea.OnResult((DrawableCatchHitObject)judgedObject, result);
    }
}
