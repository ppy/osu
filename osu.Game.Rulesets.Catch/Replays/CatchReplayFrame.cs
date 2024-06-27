// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public List<CatchAction> Actions = new List<CatchAction>();

        public float Position;
        public bool Dashing;

        public CatchReplayFrame()
        {
        }

        public CatchReplayFrame(double time, float? position = null, bool dashing = false, CatchReplayFrame? lastFrame = null)
            : base(time)
        {
            Position = position ?? -1;
            Dashing = dashing;

            if (Dashing)
                Actions.Add(CatchAction.Dash);

            if (lastFrame != null)
            {
                if (Position > lastFrame.Position)
                    lastFrame.Actions.Add(CatchAction.MoveRight);
                else if (Position < lastFrame.Position)
                    lastFrame.Actions.Add(CatchAction.MoveLeft);
            }
        }

        // Initialize coverage tracking variables
        private static bool fromLegacyDashingCheck = false;
        private static bool fromLegacyLastFrameCheck = false;
        private static bool fromLegacyMoveRight = false;
        private static bool fromLegacyMoveLeft = false;

        public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame? lastFrame = null)
        {
            Position = currentFrame.Position.X;
            Dashing = currentFrame.ButtonState == ReplayButtonState.Left1;
            fromLegacyDashingCheck = Dashing;

            if (Dashing)
            {
                Actions.Add(CatchAction.Dash);
            }
            else
            {
                //do nothing
            }

            if (lastFrame is CatchReplayFrame lastCatchFrame)
            {
                fromLegacyLastFrameCheck = true;

                if (Position > lastCatchFrame.Position)
                {
                    fromLegacyMoveRight = true;
                    lastCatchFrame.Actions.Add(CatchAction.MoveRight);
                }
                else if (Position < lastCatchFrame.Position)
                {
                    fromLegacyMoveLeft = true;
                    lastCatchFrame.Actions.Add(CatchAction.MoveLeft);
                }
                else
                {
                    fromLegacyMoveRight = false;
                    fromLegacyMoveLeft = false;
                }
            }
            else
            {
                fromLegacyLastFrameCheck = false;
            }

            PrintCoverage();
        }

        public static void PrintCoverage()
        {
            System.Console.WriteLine("F2Br1D was {0}", fromLegacyDashingCheck ? "hit" : "not hit");
            System.Console.WriteLine("F2Br2D was {0}", fromLegacyLastFrameCheck ? "hit" : "not hit");
            System.Console.WriteLine("F2Br3D was {0}", fromLegacyMoveRight ? "hit" : "not hit");
            System.Console.WriteLine("F2Br4D was {0}", fromLegacyMoveLeft ? "hit" : "not hit");
        }


        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            ReplayButtonState state = ReplayButtonState.None;

            if (Actions.Contains(CatchAction.Dash)) state |= ReplayButtonState.Left1;

            return new LegacyReplayFrame(Time, Position, null, state);
        }
    }
}
