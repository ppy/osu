// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// Base handler for editor rotation operations.
    /// </summary>
    public class SelectionRotationHandler
    {
        /// <summary>
        /// Whether the rotation can currently be performed.
        /// </summary>
        public Bindable<bool> CanRotate { get; private set; } = new BindableBool();

        public void Rotate(float rotation, Vector2? origin = null)
        {
            Begin();
            Update(rotation, origin);
            Commit();
        }

        public virtual void Begin()
        {
        }

        public virtual void Update(float rotation, Vector2? origin = null)
        {
        }

        public virtual void Commit()
        {
        }
    }
}
