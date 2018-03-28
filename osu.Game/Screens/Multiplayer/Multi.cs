// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Multiplayer
{
    public class Multi : OsuScreen
    {
        private readonly Header header;
        private readonly Container content;

        public Multi()
        {
            Lobby lobby;
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
                    Child = lobby = new Lobby(),
                },
            };

            header.CurrentScreen = lobby;
            lobby.Exited += s => Exit();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            content.Padding = new MarginPadding { Top = header.DrawHeight };
        }
    }
}
