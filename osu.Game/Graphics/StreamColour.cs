// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;

namespace osu.Game.Graphics
{
    public class StreamColour
    {
        public readonly Color4 Stable = new Color4(102, 204, 255, 255);
        public readonly Color4 StableFallback = new Color4(34, 153, 187, 255);
        public readonly Color4 Beta = new Color4(255, 221, 85, 255);
        public readonly Color4 CuttingEdge = new Color4(238, 170, 0, 255);
        public readonly Color4 Lazer = new Color4(237, 18, 33, 255);
        public readonly Color4 Web = new Color4(136, 102, 238, 255);
    }
}
