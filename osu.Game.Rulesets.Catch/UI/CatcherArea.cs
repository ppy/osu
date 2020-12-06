// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        public const float CATCHER_SIZE = 106.75f;

        public readonly Catcher MovableCatcher;
        private readonly CatchComboDisplay comboDisplay;

        public CatcherArea(Container droppedObjectContainer, BeatmapDifficulty difficulty = null)
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
                MovableCatcher = new Catcher(this, droppedObjectContainer, difficulty) { X = CatchPlayfield.CENTER_X },
            };
        }

        public void OnNewResult(DrawableCatchHitObject hitObject, JudgementResult result)
        {
            if (!result.Type.IsScorable())
                return;

            if (hitObject.HitObject.LastInCombo)
            {
                if (result.Judgement is CatchJudgement catchJudgement && catchJudgement.ShouldExplodeFor(result))
                    MovableCatcher.Explode();
                else
                    MovableCatcher.Drop();
            }

            comboDisplay.OnNewResult(hitObject, result);
        }

        public void OnRevertResult(DrawableCatchHitObject fruit, JudgementResult result)
            => comboDisplay.OnRevertResult(fruit, result);

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
