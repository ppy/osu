// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osuTK.Graphics;

namespace osu.Game.Graphics
{
    public class OsuColour
    {
        public static Color4 Gray(float amt) => new Color4(amt, amt, amt, 1f);
        public static Color4 Gray(byte amt) => new Color4(amt, amt, amt, 255);

        public static Color4 FromHex(string hex)
        {
            if (hex[0] == '#')
                hex = hex.Substring(1);

            switch (hex.Length)
            {
                default:
                    throw new ArgumentException(@"Invalid hex string length!");

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

        public Color4 ForDifficultyRating(DifficultyRating difficulty)
        {
            switch (difficulty)
            {
                case DifficultyRating.Easy:
                    return Green;

                default:
                case DifficultyRating.Normal:
                    return Blue;

                case DifficultyRating.Hard:
                    return Yellow;

                case DifficultyRating.Insane:
                    return Pink;

                case DifficultyRating.Expert:
                    return Purple;

                case DifficultyRating.ExpertPlus:
                    return Gray0;
            }
        }

        // See https://github.com/ppy/osu-web/blob/master/resources/assets/less/colors.less
        public readonly Color4 PurpleLighter = FromHex(@"eeeeff");
        public readonly Color4 PurpleLight = FromHex(@"aa88ff");
        public readonly Color4 PurpleLightAlternative = FromHex(@"cba4da");
        public readonly Color4 Purple = FromHex(@"8866ee");
        public readonly Color4 PurpleDark = FromHex(@"6644cc");
        public readonly Color4 PurpleDarkAlternative = FromHex(@"312436");
        public readonly Color4 PurpleDarker = FromHex(@"441188");

        public readonly Color4 PinkLighter = FromHex(@"ffddee");
        public readonly Color4 PinkLight = FromHex(@"ff99cc");
        public readonly Color4 Pink = FromHex(@"ff66aa");
        public readonly Color4 PinkDark = FromHex(@"cc5288");
        public readonly Color4 PinkDarker = FromHex(@"bb1177");

        public readonly Color4 BlueLighter = FromHex(@"ddffff");
        public readonly Color4 BlueLight = FromHex(@"99eeff");
        public readonly Color4 Blue = FromHex(@"66ccff");
        public readonly Color4 BlueDark = FromHex(@"44aadd");
        public readonly Color4 BlueDarker = FromHex(@"2299bb");

        public readonly Color4 YellowLighter = FromHex(@"ffffdd");
        public readonly Color4 YellowLight = FromHex(@"ffdd55");
        public readonly Color4 Yellow = FromHex(@"ffcc22");
        public readonly Color4 YellowDark = FromHex(@"eeaa00");
        public readonly Color4 YellowDarker = FromHex(@"cc6600");

        public readonly Color4 GreenLighter = FromHex(@"eeffcc");
        public readonly Color4 GreenLight = FromHex(@"b3d944");
        public readonly Color4 Green = FromHex(@"88b300");
        public readonly Color4 GreenDark = FromHex(@"668800");
        public readonly Color4 GreenDarker = FromHex(@"445500");

        public readonly Color4 Sky = FromHex(@"6bb5ff");
        public readonly Color4 GreySkyLighter = FromHex(@"c6e3f4");
        public readonly Color4 GreySkyLight = FromHex(@"8ab3cc");
        public readonly Color4 GreySky = FromHex(@"405461");
        public readonly Color4 GreySkyDark = FromHex(@"303d47");
        public readonly Color4 GreySkyDarker = FromHex(@"21272c");

        public readonly Color4 Seafoam = FromHex(@"05ffa2");
        public readonly Color4 GreySeafoamLighter = FromHex(@"9ebab1");
        public readonly Color4 GreySeafoamLight = FromHex(@"4d7365");
        public readonly Color4 GreySeafoam = FromHex(@"33413c");
        public readonly Color4 GreySeafoamDark = FromHex(@"2c3532");
        public readonly Color4 GreySeafoamDarker = FromHex(@"1e2422");

        public readonly Color4 Cyan = FromHex(@"05f4fd");
        public readonly Color4 GreyCyanLighter = FromHex(@"77b1b3");
        public readonly Color4 GreyCyanLight = FromHex(@"436d6f");
        public readonly Color4 GreyCyan = FromHex(@"293d3e");
        public readonly Color4 GreyCyanDark = FromHex(@"243536");
        public readonly Color4 GreyCyanDarker = FromHex(@"1e2929");

        public readonly Color4 Lime = FromHex(@"82ff05");
        public readonly Color4 GreyLimeLighter = FromHex(@"deff87");
        public readonly Color4 GreyLimeLight = FromHex(@"657259");
        public readonly Color4 GreyLime = FromHex(@"3f443a");
        public readonly Color4 GreyLimeDark = FromHex(@"32352e");
        public readonly Color4 GreyLimeDarker = FromHex(@"2e302b");

        public readonly Color4 Violet = FromHex(@"bf04ff");
        public readonly Color4 GreyVioletLighter = FromHex(@"ebb8fe");
        public readonly Color4 GreyVioletLight = FromHex(@"685370");
        public readonly Color4 GreyViolet = FromHex(@"46334d");
        public readonly Color4 GreyVioletDark = FromHex(@"2c2230");
        public readonly Color4 GreyVioletDarker = FromHex(@"201823");

        public readonly Color4 Carmine = FromHex(@"ff0542");
        public readonly Color4 GreyCarmineLighter = FromHex(@"deaab4");
        public readonly Color4 GreyCarmineLight = FromHex(@"644f53");
        public readonly Color4 GreyCarmine = FromHex(@"342b2d");
        public readonly Color4 GreyCarmineDark = FromHex(@"302a2b");
        public readonly Color4 GreyCarmineDarker = FromHex(@"241d1e");

        public readonly Color4 Gray0 = FromHex(@"000");
        public readonly Color4 Gray1 = FromHex(@"111");
        public readonly Color4 Gray2 = FromHex(@"222");
        public readonly Color4 Gray3 = FromHex(@"333");
        public readonly Color4 Gray4 = FromHex(@"444");
        public readonly Color4 Gray5 = FromHex(@"555");
        public readonly Color4 Gray6 = FromHex(@"666");
        public readonly Color4 Gray7 = FromHex(@"777");
        public readonly Color4 Gray8 = FromHex(@"888");
        public readonly Color4 Gray9 = FromHex(@"999");
        public readonly Color4 GrayA = FromHex(@"aaa");
        public readonly Color4 GrayB = FromHex(@"bbb");
        public readonly Color4 GrayC = FromHex(@"ccc");
        public readonly Color4 GrayD = FromHex(@"ddd");
        public readonly Color4 GrayE = FromHex(@"eee");
        public readonly Color4 GrayF = FromHex(@"fff");

        public readonly Color4 RedLighter = FromHex(@"ffeded");
        public readonly Color4 RedLight = FromHex(@"ed7787");
        public readonly Color4 Red = FromHex(@"ed1121");
        public readonly Color4 RedDark = FromHex(@"ba0011");
        public readonly Color4 RedDarker = FromHex(@"870000");

        public readonly Color4 ChatBlue = FromHex(@"17292e");

        public readonly Color4 ContextMenuGray = FromHex(@"223034");
    }
}
