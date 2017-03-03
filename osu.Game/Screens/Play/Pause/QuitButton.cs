// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Graphics.UserInterface;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play.Pause
{
    public class QuitButton : DialogButton
    {
        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            ButtonColour = new Color4(170, 27, 39, 255); // The red from the design isn't in the palette so it's used directly
            SampleHover = audio.Sample.Get(@"Menu/menuclick");
            SampleClick = audio.Sample.Get(@"Menu/menuback");
        }

        public QuitButton()
        {
            Text = @"Quit to Main Menu";
        }
    }
}
