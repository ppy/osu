// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Rulesets.UI
{
    public class GameplayCursorContainer : CursorContainer
    {
        /// <summary>
        /// Because Show/Hide are executed by a parent, <see cref="VisibilityContainer.State"/> is updated immediately even if the cursor
        /// is in a non-updating state (via <see cref="FrameStabilityContainer"/> limitations).
        ///
        /// This holds the true visibility value.
        /// </summary>
        public Visibility LastFrameState;

        protected override void Update()
        {
            base.Update();
            LastFrameState = State.Value;
        }
    }
}
