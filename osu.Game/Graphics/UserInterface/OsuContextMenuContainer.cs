// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuContextMenuContainer : ContextMenuContainer
    {
        protected override ContextMenu CreateContextMenu() => new OsuContextMenu();
    }

    public class OsuContextMenu : ContextMenu
    {
        public OsuContextMenu()
        {
            FadeDuration = 250;
            CornerRadius = 5;
            ItemsContainer.Padding = new MarginPadding { Vertical = OsuContextMenuItem.MARGIN_VERTICAL };
            Masking = true;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.25f),
                Radius = 4,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Background.Colour = colours.ContextGray;
        }

        protected override void AnimateOpen() => FadeIn(FadeDuration, EasingTypes.OutQuint);
        protected override void AnimateClose() => FadeOut(FadeDuration, EasingTypes.OutQuint);
    }
}
