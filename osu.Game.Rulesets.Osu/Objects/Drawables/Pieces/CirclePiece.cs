// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class CirclePiece : Container, IKeyBindingHandler<OsuAction>
    {
        private readonly Sprite disc;

        public Func<bool> Hit;

        public CirclePiece()
        {
            Size = new Vector2((float)OsuHitObject.OBJECT_RADIUS * 2);
            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                disc = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                new TrianglesPiece
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingMode.Additive,
                    Alpha = 0.5f,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            disc.Texture = textures.Get(@"Play/osu/disc");
        }

        public bool OnPressed(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    return IsHovered && (Hit?.Invoke() ?? false);
            }

            return false;
        }

        public bool OnReleased(OsuAction action) => false;
    }
}