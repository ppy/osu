﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuContextMenu : OsuMenu
    {
        private const int fade_duration = 250;

        public OsuContextMenu()
            : base(Direction.Vertical)
        {
            MaskingContainer.CornerRadius = 5;
            MaskingContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.1f),
                Radius = 4,
            };

            ItemsContainer.Padding = new MarginPadding { Vertical = DrawableOsuMenuItem.MARGIN_VERTICAL };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.ContextMenuGray;
        }

        protected override void AnimateOpen() => this.FadeIn(fade_duration, Easing.OutQuint);
        protected override void AnimateClose() => this.FadeOut(fade_duration, Easing.OutQuint);
    }
}
