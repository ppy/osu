using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableDrumRoll : DrawableTaikoHitObject
    {
        private static Color4 yellow_colour = new Color4(238, 170, 0, 255);

        private DrumRoll drumRoll;

        private Container body;

        public DrawableDrumRoll(DrumRoll drumRoll)
            : base(drumRoll)
        {
            this.drumRoll = drumRoll;

            Size = new Vector2((float)drumRoll.Length * drumRoll.RepeatCount, 128);

            Children = new Drawable[]
            {
                body = new Container()
                {
                    RelativeSizeAxes = Axes.Both,

                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 8,
                    
                    EdgeEffect = new EdgeEffect()
                    {
                        Colour = new Color4(yellow_colour.R, yellow_colour.G, yellow_colour.B, 0.75f),
                        Radius = 50,
                        Type = EdgeEffectType.Glow,
                    },

                    CornerRadius = 128 / 2,

                    Children = new Drawable[]
                    {
                        new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = yellow_colour
                        },
                        new DrumRollTrianglesPiece()
                        {
                            RelativeSizeAxes = Axes.Both,

                            ColourDark = yellow_colour.Darken(0.1f),
                            ColourLight = yellow_colour.Darken(0.05f),
                            Alpha = 0.75f,
                        }
                    }
                }    
            };
        }
    }
}
