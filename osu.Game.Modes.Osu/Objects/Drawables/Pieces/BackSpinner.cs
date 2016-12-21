using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class BackSpinner : Container
    {

        public override bool HandleInput => false;
        private Container cont;
        private Box backBox;

        const float size = 400;
        public BackSpinner(Spinner spinner)
        {
            Position = spinner.Position;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                cont = new Container
                {
                    Masking = true,
                    Colour = spinner.Colour,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = size/2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 1,
                    Children = new[]
                    {
                        backBox = new Box
                        {
                            Colour = spinner.Colour,
                            Alpha = 0.5f,
                            Width = size,
                            Height = size,
                        }
                    },
                },
            };
        }
    }
}
