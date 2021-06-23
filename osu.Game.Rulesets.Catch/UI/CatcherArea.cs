// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container, IKeyBindingHandler<CatchAction>
    {
        public const float CATCHER_SIZE = 106.75f;

        public readonly Catcher MovableCatcher;
        private readonly CatchComboDisplay comboDisplay;

        /// <summary>
        /// <c>-1</c> when only left button is pressed.
        /// <c>1</c> when only right button is pressed.
        /// <c>0</c> when none or both left and right buttons are pressed.
        /// </summary>
        private int currentDirection;

        public CatcherArea(Container<CaughtObject> droppedObjectContainer, BeatmapDifficulty difficulty = null)
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
            MovableCatcher.OnNewResult(hitObject, result);

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

        public void OnRevertResult(DrawableCatchHitObject hitObject, JudgementResult result)
        {
            comboDisplay.OnRevertResult(hitObject, result);
            MovableCatcher.OnRevertResult(hitObject, result);
        }

        protected override void Update()
        {
            base.Update();

            var replayState = (GetContainingInputManager().CurrentState as RulesetInputManagerInputState<CatchAction>)?.LastReplayState as CatchFramedReplayInputHandler.CatchReplayState;

            SetCatcherPosition(
                replayState?.CatcherX ??
                (float)(MovableCatcher.X + MovableCatcher.Speed * currentDirection * Clock.ElapsedFrameTime));
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            comboDisplay.X = MovableCatcher.X;
        }

        public void SetCatcherPosition(float X)
        {
            float lastPosition = MovableCatcher.X;
            float newPosition = Math.Clamp(X, 0, CatchPlayfield.WIDTH);

            MovableCatcher.X = newPosition;

            if (lastPosition < newPosition)
                MovableCatcher.VisualDirection = Direction.Right;
            else if (lastPosition > newPosition)
                MovableCatcher.VisualDirection = Direction.Left;
        }

        public bool OnPressed(CatchAction action)
        {
            switch (action)
            {
                case CatchAction.MoveLeft:
                    currentDirection--;
                    return true;

                case CatchAction.MoveRight:
                    currentDirection++;
                    return true;

                case CatchAction.Dash:
                    MovableCatcher.Dashing = true;
                    return true;
            }

            return false;
        }

        public void OnReleased(CatchAction action)
        {
            switch (action)
            {
                case CatchAction.MoveLeft:
                    currentDirection++;
                    break;

                case CatchAction.MoveRight:
                    currentDirection--;
                    break;

                case CatchAction.Dash:
                    MovableCatcher.Dashing = false;
                    break;
            }
        }
    }
}
