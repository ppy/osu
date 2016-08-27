//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace osu.Framework.Graphics.Containers
{
    public class MaskingContainer : Container
    {
        protected override void PreDraw()
        {
            GLWrapper.PushScissor(ScreenSpaceDrawQuad.BoundingRectangle);
        }

        protected override void PostDraw()
        {
            GLWrapper.PopScissor();
        }
    }
}
