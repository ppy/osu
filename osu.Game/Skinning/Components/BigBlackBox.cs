// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Components
{
    /// <summary>
    /// Intended to be a test bed for skinning. May be removed at some point in the future.
    /// </summary>
    [UsedImplicitly]
    public class BigBlackBox : CompositeDrawable, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        public BigBlackBox()
        {
            Size = new Vector2(150);

            Masking = true;
            CornerRadius = 20;
            CornerExponent = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },

                new OsuSpriteText
                {
                    Text = "Big Black Box",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }
    }
}
