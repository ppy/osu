// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;

namespace osu.Game.Graphics
{
    public class OsuColour
    {
        public static Color4 Gray(float amt) => new Color4(amt, amt, amt, 1f);
        public static Color4 Gray(byte amt) => new Color4(amt, amt, amt, 255);

        public static Color4 FromHex(string hex)
        {
            switch (hex.Length)
            {
                default:
                    throw new Exception(@"Invalid hex string length!");
                case 3:
                    return new Color4(
                        (byte)(Convert.ToByte(hex.Substring(0, 1), 16) * 17),
                        (byte)(Convert.ToByte(hex.Substring(1, 1), 16) * 17),
                        (byte)(Convert.ToByte(hex.Substring(2, 1), 16) * 17),
                        255);
                case 6:
                    return new Color4(
                        Convert.ToByte(hex.Substring(0, 2), 16),
                        Convert.ToByte(hex.Substring(2, 2), 16),
                        Convert.ToByte(hex.Substring(4, 2), 16),
                        255);
            }
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

        public Color4 Gray0 = FromHex(@"000");
        public Color4 Gray1 = FromHex(@"111");
        public Color4 Gray2 = FromHex(@"222");
        public Color4 Gray3 = FromHex(@"333");
        public Color4 Gray4 = FromHex(@"444");
        public Color4 Gray5 = FromHex(@"555");
        public Color4 Gray6 = FromHex(@"666");
        public Color4 Gray7 = FromHex(@"777");
        public Color4 Gray8 = FromHex(@"888");
        public Color4 Gray9 = FromHex(@"999");
        public Color4 GrayA = FromHex(@"aaa");
        public Color4 GrayB = FromHex(@"bbb");
        public Color4 GrayC = FromHex(@"ccc");
        public Color4 GrayD = FromHex(@"ddd");
        public Color4 GrayE = FromHex(@"eee");
        public Color4 GrayF = FromHex(@"fff");

        public Color4 RedLighter = FromHex(@"ffeded");
        public Color4 RedLight = FromHex(@"ed7787");
        public Color4 Red = FromHex(@"ed1121");
        public Color4 RedDark = FromHex(@"ba0011");
        public Color4 RedDarker = FromHex(@"870000");
    }
}
