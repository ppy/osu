using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        private DrumRollTick drumRollTick;

        public DrawableDrumRollTick(DrumRollTick drumRollTick)
            : base(drumRollTick)
        {
            this.drumRollTick = drumRollTick;

            Size = new Vector2(16) * drumRollTick.Scale;

            Masking = true;
            CornerRadius = Size.X / 2;

            Origin = Anchor.Centre;
            Anchor = Anchor.CentreLeft;

            BorderThickness = 3;
            BorderColour = Color4.White;

            Children = new[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = drumRollTick.FirstTick ? 1f : 0f,
                    AlwaysPresent = true
                }
            };
        }
    }
}
