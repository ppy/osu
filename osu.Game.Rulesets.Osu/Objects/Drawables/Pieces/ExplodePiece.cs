﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class ExplodePiece : Container
    {
        public ExplodePiece()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;
            Alpha = 0;

            Child = new TrianglesPiece
            {
                Blending = BlendingParameters.Additive,
                RelativeSizeAxes = Axes.Both,
                Alpha = 0.2f,
            };
        }
    }
}
