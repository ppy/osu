//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class CirclePiece : Container
    {

        private Sprite disc;
        private Triangles triangles;

        public Func<bool> Hit;

        public CirclePiece()
        {
            Size = new Vector2(144);
            Masking = true;
            CornerRadius = DrawSize.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                disc = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                triangles = new Triangles
                {
                    BlendingMode = BlendingMode.Additive,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            disc.Texture = textures.Get(@"Play/osu/disc@2x");
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            return Hit?.Invoke() ?? false;
        }
    }
}