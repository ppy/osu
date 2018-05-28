// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Multi.Screens.Lounge;

namespace osu.Game.Screens.Multi
{
    public class Multiplayer : OsuScreen
    {
        private readonly MultiplayerWaveContainer waves;

        protected override Container<Drawable> Content => waves;

        public Multiplayer()
        {
            InternalChild = waves = new MultiplayerWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
            };

            Lounge lounge;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"3e3a44"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = OsuColour.FromHex(@"3c3842"),
                            ColourDark = OsuColour.FromHex(@"393540"),
                            TriangleScale = 5,
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT },
                    Child = lounge = new Lounge(),
                },
                new Header(lounge),
            };

            lounge.Exited += s => Exit();
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            waves.Show();
        }

        protected override bool OnExiting(Screen next)
        {
            waves.Hide();
            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            waves.Show();
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            waves.Hide();
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            // the wave overlay transition takes longer than expected to run.
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
            base.LogoExiting(logo);
        }

        private class MultiplayerWaveContainer : WaveContainer
        {
            protected override bool StartHidden => true;

            public MultiplayerWaveContainer()
            {
                FirstWaveColour = OsuColour.FromHex(@"654d8c");
                SecondWaveColour = OsuColour.FromHex(@"554075");
                ThirdWaveColour = OsuColour.FromHex(@"44325e");
                FourthWaveColour = OsuColour.FromHex(@"392850");
            }
        }
    }
}
