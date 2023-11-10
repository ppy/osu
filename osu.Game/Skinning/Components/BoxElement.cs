// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Components
{
    public partial class BoxElement : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Corner rounding", "How round the corners of the box should be.")]
        public BindableFloat CornerRounding { get; } = new BindableFloat(1)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        public BoxElement()
        {
            Size = new Vector2(400, 80);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                },
            };

            Masking = true;
        }

        protected override void Update()
        {
            base.Update();

            CornerRadius = CornerRounding.Value * Math.Min(DrawWidth, DrawHeight) * 0.5f;
        }
    }
}
