// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ExpandDetailsButton : ProfileHeaderButton
    {
        public readonly BindableBool DetailsVisible = new BindableBool();

        public override string TooltipText => DetailsVisible.Value ? "collapse" : "expand";

        private SpriteIcon icon;

        public ExpandDetailsButton()
        {
            Action = () => DetailsVisible.Toggle();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.GreySeafoamLight;
            HoverColour = colours.GreySeafoamLight.Darken(0.2f);

            Child = icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(20, 12)
            };

            DetailsVisible.BindValueChanged(visible => updateState(visible.NewValue), true);
        }

        private void updateState(bool detailsVisible) => icon.Icon = detailsVisible ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
    }
}
