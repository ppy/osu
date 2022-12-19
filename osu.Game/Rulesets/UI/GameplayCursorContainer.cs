// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osuTK;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using System.Linq;

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
        public Vector2 TargetPosition = Vector2.Zero;
        public Mod[] Mods;

        public GameplayCursorContainer(Mod[] mods)
        {
            Mods = mods;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            TargetPosition = e.MousePosition;
            return true;
        }

        protected override void Update()
        {
            base.Update();
            Position = TargetPosition;
            foreach (IModifiesCursorMovement modifier in Mods.OfType<IModifiesCursorMovement>())
            {
                Position = modifier.UpdatePosition(TargetPosition, (float)Time.Elapsed / 1000f);
            }
        }
    }
}
