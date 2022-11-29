// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class SelectionBoxScaleHandle : SelectionBoxDragHandle
    {
        public Action<Vector2, Anchor> HandleScale { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(10);
        }

        protected override void OnDrag(DragEvent e)
        {
            HandleScale?.Invoke(e.Delta, Anchor);
            base.OnDrag(e);
        }
    }
}
