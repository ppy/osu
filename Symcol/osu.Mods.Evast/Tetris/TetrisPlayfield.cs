// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Mods.Evast.Tetris
{
    public class TetrisPlayfiled : Container
    {
        private const int spacing = 50;

        private readonly MainPlayfield mainPlayfield;
        private readonly PixelField nextFigureField;

        public void Restart() => mainPlayfield.Restart();
        public void Pause() => mainPlayfield.Pause();
        public void Stop() => mainPlayfield.Stop();
        public void Continue() => mainPlayfield.Continue();

        public double UpdateDelay
        {
            set { mainPlayfield.UpdateDelay = value; }
            get { return mainPlayfield.UpdateDelay; }
        }

        public TetrisPlayfiled(int xCount, int yCount, int pixelSize = 15)
        {
            AutoSizeAxes = Axes.Y;
            Width = xCount * pixelSize + 4 * pixelSize + spacing;
            Children = new Drawable[]
            {
                mainPlayfield = new MainPlayfield(xCount, yCount, pixelSize)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                nextFigureField = new PixelField(4, 4, pixelSize)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                }
            };

            nextFigureField.Add(new OsuSpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.BottomCentre,
                Margin = new MarginPadding { Bottom = 10 },
                Text = "Next figure"
            });
        }

        private class MainPlayfield : PixelField
        {
            public MainPlayfield(int xCount, int yCount, int pixelSize = 15)
                : base(xCount, yCount, pixelSize)
            {
            }
        }
    }
}
