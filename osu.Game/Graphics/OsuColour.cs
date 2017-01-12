using System;
using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Graphics
{
    public static class OsuColour
    {
        public static Color4 Opacity(this Color4 color, float a) => new Color4(color.R, color.G, color.B, a);
        public static Color4 Opacity(this Color4 color, byte a) => new Color4(color.R, color.G, color.B, a / 255f);
        public static Color4 Gray(float amt) => new Color4(amt, amt, amt, 1f);
        public static Color4 Gray(byte amt) => new Color4(amt, amt, amt, 255);

        private static Color4 FromHex(string hex)
        {
            return new Color4(
                Convert.ToByte(hex.Substring(1, 2), 16),
                Convert.ToByte(hex.Substring(3, 2), 16),
                Convert.ToByte(hex.Substring(5, 2), 16),
                255);
        }

        // See https://github.com/ppy/osu-web/blob/master/resources/assets/less/colors.less

        public static readonly Color4 PurpleLighter = FromHex(@"eeeeff");
        public static readonly Color4 PurpleLight = FromHex(@"aa88ff");
        public static readonly Color4 Purple = FromHex(@"8866ee");
        public static readonly Color4 PurpleDark = FromHex(@"6644cc");
        public static readonly Color4 PurpleDarker = FromHex(@"441188");

        public static readonly Color4 PinkLighter = FromHex(@"ffddee");
        public static readonly Color4 PinkLight = FromHex(@"ff99cc");
        public static readonly Color4 Pink = FromHex(@"ff66aa");
        public static readonly Color4 PinkDark = FromHex(@"cc5288");
        public static readonly Color4 PinkDarker = FromHex(@"bb1177");

        public static readonly Color4 BlueLighter = FromHex(@"ddffff");
        public static readonly Color4 BlueLight = FromHex(@"99eeff");
        public static readonly Color4 Blue = FromHex(@"66ccff");
        public static readonly Color4 BlueDark = FromHex(@"44aadd");
        public static readonly Color4 BlueDarker = FromHex(@"2299bb");

        public static readonly Color4 YellowLighter = FromHex(@"ffffdd");
        public static readonly Color4 YellowLight = FromHex(@"ffdd55");
        public static readonly Color4 Yellow = FromHex(@"ffcc22");
        public static readonly Color4 YellowDark = FromHex(@"eeaa00");
        public static readonly Color4 YellowDarker = FromHex(@"cc6600");

        public static readonly Color4 GreenLighter = FromHex(@"eeffcc");
        public static readonly Color4 GreenLight = FromHex(@"b3d944");
        public static readonly Color4 Green = FromHex(@"88b300");
        public static readonly Color4 GreenDark = FromHex(@"668800");
        public static readonly Color4 GreenDarker = FromHex(@"445500");

        public static readonly Color4 Red = FromHex(@"fc4549");
    }
}
