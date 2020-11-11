// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        public const float CATCHER_SIZE = 106.75f;

        public Func<CatchHitObject, DrawableHitObject<CatchHitObject>> CreateDrawableRepresentation;

        public readonly Catcher MovableCatcher;
        private readonly CatchComboDisplay comboDisplay;

        public Container ExplodingFruitTarget
        {
            set => MovableCatcher.ExplodingFruitTarget = value;
        }

        private DrawableCatchHitObject lastPlateableFruit;

        public CatcherArea(BeatmapDifficulty difficulty = null)
        {
            Size = new Vector2(CatchPlayfield.WIDTH, CATCHER_SIZE);
            Children = new Drawable[]
            {
                comboDisplay = new CatchComboDisplay
                {
                    RelativeSizeAxes = Axes.None,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Bottom = 350f },
                    X = CatchPlayfield.CENTER_X
                },
                MovableCatcher = new Catcher(this, difficulty) { X = CatchPlayfield.CENTER_X },
            };
        }

        public void OnNewResult(DrawableCatchHitObject fruit, JudgementResult result)
        {
            if (!result.Type.IsScorable())
                return;

            void runAfterLoaded(Action action)
            {
                if (lastPlateableFruit == null)
                    return;

                // this is required to make this run after the last caught fruit runs updateState() at least once.
                // TODO: find a better alternative
                if (lastPlateableFruit.IsLoaded)
                    action();
                else
                    lastPlateableFruit.OnLoadComplete += _ => action();
            }

            if (result.IsHit && fruit.HitObject.CanBePlated)
            {
                // create a new (cloned) fruit to stay on the plate. the original is faded out immediately.
                var caughtFruit = (DrawableCatchHitObject)CreateDrawableRepresentation?.Invoke(fruit.HitObject);

                if (caughtFruit == null) return;

                caughtFruit.RelativePositionAxes = Axes.None;
                caughtFruit.Position = new Vector2(MovableCatcher.ToLocalSpace(fruit.ScreenSpaceDrawQuad.Centre).X - MovableCatcher.DrawSize.X / 2, 0);
                caughtFruit.IsOnPlate = true;

                caughtFruit.Anchor = Anchor.TopCentre;
                caughtFruit.Origin = Anchor.Centre;
                caughtFruit.Scale *= 0.5f;
                caughtFruit.LifetimeStart = caughtFruit.HitObject.StartTime;
                caughtFruit.LifetimeEnd = double.MaxValue;

                MovableCatcher.PlaceOnPlate(caughtFruit);
                lastPlateableFruit = caughtFruit;

                if (!fruit.StaysOnPlate)
                    runAfterLoaded(() => MovableCatcher.Explode(caughtFruit));
            }

            if (fruit.HitObject.LastInCombo)
            {
                if (result.Judgement is CatchJudgement catchJudgement && catchJudgement.ShouldExplodeFor(result))
                    runAfterLoaded(() => MovableCatcher.Explode());
                else
                    MovableCatcher.Drop();
            }

            comboDisplay.OnNewResult(fruit, result);
        }

        public void OnRevertResult(DrawableCatchHitObject fruit, JudgementResult result)
            => comboDisplay.OnRevertResult(fruit, result);

        public void OnReleased(CatchAction action)
        {
        }

        public bool AttemptCatch(CatchHitObject obj)
        {
            return MovableCatcher.AttemptCatch(obj);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var state = (GetContainingInputManager().CurrentState as RulesetInputManagerInputState<CatchAction>)?.LastReplayState as CatchFramedReplayInputHandler.CatchReplayState;

            if (state?.CatcherX != null)
                MovableCatcher.X = state.CatcherX.Value;

            comboDisplay.X = MovableCatcher.X;
        }
    }
}
