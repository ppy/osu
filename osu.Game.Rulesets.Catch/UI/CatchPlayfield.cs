// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// The width of the playfield.
        /// The horizontal movement of the catcher is confined in the area of this width.
        /// </summary>
        public const float WIDTH = 512;

        /// <summary>
        /// The center position of the playfield.
        /// </summary>
        public const float CENTER_X = WIDTH / 2;

        internal readonly CatcherArea CatcherArea;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            // only check the X position; handle all vertical space.
            base.ReceivePositionalInputAt(new Vector2(screenSpacePos.X, ScreenSpaceDrawQuad.Centre.Y));

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
