// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    public class ElongatedCirclePiece : CirclePiece
    {
        /// <summary>
        /// As we are being used to define the absolute size of hits, we need to be given a relative reference of our containing playfield container.
        /// </summary>
        public Func<float> PlayfieldLengthReference;

        /// <summary>
        /// The length of this piece as a multiple of the value returned by <see cref="PlayfieldLengthReference"/>
        /// </summary>
        public float Length;

        public ElongatedCirclePiece(bool isStrong = false) : base(isStrong)
        {
        }

        protected override void Update()
        {
            base.Update();

            var padding = Content.DrawHeight * Content.Width / 2;

            Content.Padding = new MarginPadding
            {
                Left = padding,
                Right = padding,
            };

            Width = (PlayfieldLengthReference?.Invoke() ?? 0) * Length + DrawHeight;
        }
    }
}
