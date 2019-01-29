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
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield
    {
        public const float BASE_WIDTH = 512;

        internal readonly CatcherArea CatcherArea;

        public CatchPlayfield(BeatmapDifficulty difficulty, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> getVisualRepresentation)
        {
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
                    CatcherArea = new CatcherArea(difficulty)
                    {
                        GetVisualRepresentation = getVisualRepresentation,
                        ExplodingFruitTarget = explodingFruitContainer,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.TopLeft,
                    },
                    HitObjectContainer
                }
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
