// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Comments.Buttons
{
    public partial class ChevronButton : OsuHoverContainer
    {
        public readonly BindableBool Expanded = new BindableBool(true);

        private readonly SpriteIcon icon;

        public ChevronButton()
        {
            Size = new Vector2(40, 22);
            Child = icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(12),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            IdleColour = HoverColour = colourProvider.Foreground1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Action = Expanded.Toggle;
            Expanded.BindValueChanged(onExpandedChanged, true);
        }

        private void onExpandedChanged(ValueChangedEvent<bool> expanded)
        {
            icon.Icon = expanded.NewValue ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
        }
    }
}
