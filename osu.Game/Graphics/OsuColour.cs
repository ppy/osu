using System;
using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Graphics
{
    public static class OsuColourExtensions
    {
        public static Color4 Opacity(this Color4 color, float a) => new Color4(color.R, color.G, color.B, a);
        public static Color4 Opacity(this Color4 color, byte a) => new Color4(color.R, color.G, color.B, a / 255f);
    }

    public class OsuColour
    {
        public static Color4 Gray(float amt) => new Color4(amt, amt, amt, 1f);
        public static Color4 Gray(byte amt) => new Color4(amt, amt, amt, 255);

        private static Color4 FromHex(string hex)
        {
            return new Color4(
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16),
                255);
        }

        // See https://github.com/ppy/osu-web/blob/master/resources/assets/less/colors.less

        public Color4 PurpleLighter = FromHex(@"eeeeff");
        public Color4 PurpleLight = FromHex(@"aa88ff");
        public Color4 Purple = FromHex(@"8866ee");
        public Color4 PurpleDark = FromHex(@"6644cc");
        public Color4 PurpleDarker = FromHex(@"441188");

        public Color4 PinkLighter = FromHex(@"ffddee");
        public Color4 PinkLight = FromHex(@"ff99cc");
        public Color4 Pink = FromHex(@"ff66aa");
        public Color4 PinkDark = FromHex(@"cc5288");
        public Color4 PinkDarker = FromHex(@"bb1177");

        public Color4 BlueLighter = FromHex(@"ddffff");
        public Color4 BlueLight = FromHex(@"99eeff");
        public Color4 Blue = FromHex(@"66ccff");
        public Color4 BlueDark = FromHex(@"44aadd");
        public Color4 BlueDarker = FromHex(@"2299bb");

        public Color4 YellowLighter = FromHex(@"ffffdd");
        public Color4 YellowLight = FromHex(@"ffdd55");
        public Color4 Yellow = FromHex(@"ffcc22");
        public Color4 YellowDark = FromHex(@"eeaa00");
        public Color4 YellowDarker = FromHex(@"cc6600");

        public Color4 GreenLighter = FromHex(@"eeffcc");
        public Color4 GreenLight = FromHex(@"b3d944");
        public Color4 Green = FromHex(@"88b300");
        public Color4 GreenDark = FromHex(@"668800");
        public Color4 GreenDarker = FromHex(@"445500");

        public Color4 Red = FromHex(@"fc4549");
    }
}
