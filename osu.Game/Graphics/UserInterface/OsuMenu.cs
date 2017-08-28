// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuMenu : Menu
    {
        public OsuMenu()
        {
            CornerRadius = 4;
            BackgroundColour = Color4.Black.Opacity(0.5f);
        }

        protected override void AnimateOpen() => this.FadeIn(300, Easing.OutQuint);
        protected override void AnimateClose() => this.FadeOut(300, Easing.OutQuint);

        protected override void UpdateMenuHeight()
        {
            var actualHeight = (RelativeSizeAxes & Axes.Y) > 0 ? 1 : ContentHeight;
            this.ResizeHeightTo(State == MenuState.Opened ? actualHeight : 0, 300, Easing.OutQuint);
        }

        protected override MarginPadding ItemFlowContainerPadding => new MarginPadding(5);
    }
}
