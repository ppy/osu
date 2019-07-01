// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield
    {
        public const float BASE_WIDTH = 512;

        internal readonly CatcherArea CatcherArea;

        public CatchPlayfield(BeatmapDifficulty difficulty, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> createDrawableRepresentation)
        {
            Container explodingFruitContainer;

            InternalChildren = new Drawable[]
            {
                explodingFruitContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                CatcherArea = new CatcherArea(difficulty)
                {
                    CreateDrawableRepresentation = createDrawableRepresentation,
                    ExplodingFruitTarget = explodingFruitContainer,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                },
                HitObjectContainer
            };
        }

        public bool CheckIfWeCanCatch(CatchHitObject obj) => CatcherArea.AttemptCatch(obj);

        public override void Add(DrawableHitObject h)
        {
            h.OnNewResult += onNewResult;

            base.Add(h);

            var fruit = (DrawableCatchHitObject)h;
            fruit.CheckPosition = CheckIfWeCanCatch;
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
            => CatcherArea.OnResult((DrawableCatchHitObject)judgedObject, result);
    }
}
