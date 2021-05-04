// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class SelectionBoxRotationHandle : SelectionBoxDragHandle
    {
        private SpriteIcon icon;

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(15f);
            AddInternal(icon = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Solid.Redo,
                Scale = new Vector2
                {
                    X = Anchor.HasFlagFast(Anchor.x0) ? 1f : -1f,
                    Y = Anchor.HasFlagFast(Anchor.y0) ? 1f : -1f
                }
            });
        }

        protected override void UpdateHoverState()
        {
            base.UpdateHoverState();
            icon.FadeColour(!IsHeld && IsHovered ? Color4.White : Color4.Black, TRANSFORM_DURATION, Easing.OutQuint);
        }
    }
}
