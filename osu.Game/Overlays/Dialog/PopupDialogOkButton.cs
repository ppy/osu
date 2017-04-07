// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialogOkButton : PopupDialogButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            ButtonColour = colours.Pink;
            SampleHover = audio.Sample.Get(@"Menu/menuclick");
            SampleClick = audio.Sample.Get(@"Menu/menu-play-click");
        }
    }
}
