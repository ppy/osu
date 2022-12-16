// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osuTK;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.UI
{
    public partial class GameplayCursorContainer : CursorContainer
    {
        /// <summary>
        /// Because Show/Hide are executed by a parent, <see cref="VisibilityContainer.State"/> is updated immediately even if the cursor
        /// is in a non-updating state (via <see cref="FrameStabilityContainer"/> limitations).
        ///
        /// This holds the true visibility value.
        /// </summary>
        public Visibility LastFrameState;
        public bool Delayed;
        public Vector2 TruePosition = Vector2.Zero;
        public Vector2 TargetPosition = Vector2.Zero;
		public float FollowSpeed;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            TargetPosition = e.MousePosition;
            return true;
        }

        protected override void Update()
        {
            // FIXME: This is jank-tastic because I don't know how to modify osu!framework. But it works.
            base.Update();
            if (Delayed)
            {
                TruePosition = Vector2.Lerp(TruePosition, TargetPosition, (float)Time.Elapsed / 1000 * FollowSpeed);
            }
            else
            {
                TruePosition = TargetPosition;
            }
            ActiveCursor.Position = TruePosition;
            LastFrameState = State.Value;
        }
    }
}
