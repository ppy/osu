// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Multiplayer.Screens.Lounge;

namespace osu.Game.Screens.Multiplayer
{
    public class Multi : OsuScreen
    {
        private readonly MultiWaveContainer waves;
        private readonly Header header;
        private readonly Container content;

        protected override Container<Drawable> Content => waves;

        public Multi()
        {
            AddInternal(waves = new MultiWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                FirstWaveColour = OsuColour.FromHex(@"654d8c"),
                SecondWaveColour = OsuColour.FromHex(@"554075"),
                ThirdWaveColour = OsuColour.FromHex(@"44325e"),
                FourthWaveColour = OsuColour.FromHex(@"392850"),
            });

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
                header = new Header(),
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = lounge = new Lounge(),
                },
            };

            header.CurrentScreen = lounge;
            lounge.Exited += s => Exit();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            content.Padding = new MarginPadding { Top = header.DrawHeight };
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

        private class MultiWaveContainer : WaveContainer
        {
            protected override bool StartHidden => true;
        }
    }
}
