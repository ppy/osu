// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;
using System.Collections.Generic;

namespace osu.Game.Graphics
{
    public class StreamColour
    {
        public static readonly Color4 STABLE = new Color4(102, 204, 255, 255);
        public static readonly Color4 STABLEFALLBACK = new Color4(34, 153, 187, 255);
        public static readonly Color4 BETA = new Color4(255, 221, 85, 255);
        public static readonly Color4 CUTTINGEDGE = new Color4(238, 170, 0, 255);
        public static readonly Color4 LAZER = new Color4(237, 18, 33, 255);
        public static readonly Color4 WEB = new Color4(136, 102, 238, 255);

        private static readonly Dictionary<string, ColourInfo> colours = new Dictionary<string, ColourInfo>
        {
            { "stable40", STABLE },
            { "Stable", STABLE },
            { "stable", STABLEFALLBACK },
            { "Stable Fallback", STABLEFALLBACK },
            { "beta40", BETA },
            { "Beta", BETA },
            { "cuttingedge", CUTTINGEDGE },
            { "Cutting Edge", CUTTINGEDGE },
            { "lazer", LAZER },
            { "Lazer", LAZER },
            { "web", WEB },
        };

        public static ColourInfo FromStreamName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                if (colours.TryGetValue(name, out ColourInfo colour))
                    return colour;
            return new Color4(0, 0, 0, 255);
        }
    }
}
