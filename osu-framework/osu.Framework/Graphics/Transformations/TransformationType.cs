//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Graphics.Transformations
{
    [Flags]
    public enum TransformationType
    {
        None = 0,
        Movement = 1 << 0,
        Fade = 1 << 1,
        Scale = 1 << 2,
        Rotation = 1 << 3,
        Colour = 1 << 4,
        ParameterFlipHorizontal = 1 << 5,
        ParameterFlipVertical = 1 << 6,
        MovementX = 1 << 7,
        MovementY = 1 << 8,
        VectorScale = 1 << 9,
        ParameterAdditive = 1 << 10,
    }
}
