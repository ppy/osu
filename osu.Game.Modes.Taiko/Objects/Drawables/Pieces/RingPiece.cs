// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    public class DonRingPiece : RingPiece
    {
        protected override Drawable CreateInnerPiece()
        {
            return new CircularContainer()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Size = new Vector2(45f),

                Children = new[]
                {
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = 1
                    }
                }
            };
        }
    }

    public class KatsuRingPiece : RingPiece
    {
        protected override Drawable CreateInnerPiece()
        {
            return new CircularContainer()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Size = new Vector2(61f),

                BorderColour = Color4.White,
                BorderThickness = 8,

                Children = new[]
                {
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = 0,
                        AlwaysPresent = true
                    }
                }
            };
        }
    }

    public class SpinnerRingPiece : RingPiece
    {
        protected override Drawable CreateInnerPiece()
        {
            return new TextAwesome()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                TextSize = 45f,
                Icon = FontAwesome.fa_asterisk
            };
        }
    }

    public abstract class RingPiece : CircularContainer
    {
        public RingPiece()
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            BorderThickness = 8f;
            BorderColour = Color4.White;

            Children = new[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,

                    Alpha = 0,
                    AlwaysPresent = true
                },
                CreateInnerPiece()
            };
        }

        protected abstract Drawable CreateInnerPiece();
    }
}
