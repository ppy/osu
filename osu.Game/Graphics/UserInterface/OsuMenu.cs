// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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
            Background.Colour = Color4.Black.Opacity(0.5f);

            ItemsContainer.Padding = new MarginPadding(5);
        }

        protected override void AnimateOpen() => FadeIn(300, EasingTypes.OutQuint);

        protected override void AnimateClose() => FadeOut(300, EasingTypes.OutQuint);

        protected override void UpdateContentHeight()
        {
            var actualHeight = (RelativeSizeAxes & Axes.Y) > 0 ? 1 : ContentHeight;
            ResizeTo(new Vector2(1, State == MenuState.Opened ? actualHeight : 0), 300, EasingTypes.OutQuint);
        }
    }
}
