// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Graphics
{
    public class OsuColour
    {
        public static Colour4 Gray(float amt) => new Colour4(amt, amt, amt, 1f);
        public static Colour4 Gray(byte amt) => new Colour4(amt, amt, amt, 255);

        public Colour4 ForDifficultyRating(DifficultyRating difficulty, bool useLighterColour = false)
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
                    return useLighterColour ? PurpleLight : Purple;

                case DifficultyRating.ExpertPlus:
                    return useLighterColour ? Gray9 : Gray0;
            }
        }

        /// <summary>
        /// Retrieves the colour for a <see cref="ScoreRank"/>.
        /// </summary>
        public static Colour4 ForRank(ScoreRank rank)
        {
            switch (rank)
            {
                case ScoreRank.XH:
                case ScoreRank.X:
                    return Color4Extensions.FromHex(@"de31ae");

                case ScoreRank.SH:
                case ScoreRank.S:
                    return Color4Extensions.FromHex(@"02b5c3");

                case ScoreRank.A:
                    return Color4Extensions.FromHex(@"88da20");

                case ScoreRank.B:
                    return Color4Extensions.FromHex(@"e3b130");

                case ScoreRank.C:
                    return Color4Extensions.FromHex(@"ff8e5d");

                default:
                    return Color4Extensions.FromHex(@"ff5a5a");
            }
        }

        /// <summary>
        /// Retrieves the colour for a <see cref="HitResult"/>.
        /// </summary>
        public Colour4 ForHitResult(HitResult judgement)
        {
            switch (judgement)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                    return Blue;

                case HitResult.Ok:
                case HitResult.Good:
                    return Green;

                case HitResult.Meh:
                    return Yellow;

                case HitResult.Miss:
                    return Red;

                default:
                    return Colour4.White;
            }
        }

        // See https://github.com/ppy/osu-web/blob/master/resources/assets/less/colors.less
        public readonly Colour4 PurpleLighter = Color4Extensions.FromHex(@"eeeeff");
        public readonly Colour4 PurpleLight = Color4Extensions.FromHex(@"aa88ff");
        public readonly Colour4 PurpleLightAlternative = Color4Extensions.FromHex(@"cba4da");
        public readonly Colour4 Purple = Color4Extensions.FromHex(@"8866ee");
        public readonly Colour4 PurpleDark = Color4Extensions.FromHex(@"6644cc");
        public readonly Colour4 PurpleDarkAlternative = Color4Extensions.FromHex(@"312436");
        public readonly Colour4 PurpleDarker = Color4Extensions.FromHex(@"441188");

        public readonly Colour4 PinkLighter = Color4Extensions.FromHex(@"ffddee");
        public readonly Colour4 PinkLight = Color4Extensions.FromHex(@"ff99cc");
        public readonly Colour4 Pink = Color4Extensions.FromHex(@"ff66aa");
        public readonly Colour4 PinkDark = Color4Extensions.FromHex(@"cc5288");
        public readonly Colour4 PinkDarker = Color4Extensions.FromHex(@"bb1177");

        public readonly Colour4 BlueLighter = Color4Extensions.FromHex(@"ddffff");
        public readonly Colour4 BlueLight = Color4Extensions.FromHex(@"99eeff");
        public readonly Colour4 Blue = Color4Extensions.FromHex(@"66ccff");
        public readonly Colour4 BlueDark = Color4Extensions.FromHex(@"44aadd");
        public readonly Colour4 BlueDarker = Color4Extensions.FromHex(@"2299bb");

        public readonly Colour4 YellowLighter = Color4Extensions.FromHex(@"ffffdd");
        public readonly Colour4 YellowLight = Color4Extensions.FromHex(@"ffdd55");
        public readonly Colour4 Yellow = Color4Extensions.FromHex(@"ffcc22");
        public readonly Colour4 YellowDark = Color4Extensions.FromHex(@"eeaa00");
        public readonly Colour4 YellowDarker = Color4Extensions.FromHex(@"cc6600");

        public readonly Colour4 GreenLighter = Color4Extensions.FromHex(@"eeffcc");
        public readonly Colour4 GreenLight = Color4Extensions.FromHex(@"b3d944");
        public readonly Colour4 Green = Color4Extensions.FromHex(@"88b300");
        public readonly Colour4 GreenDark = Color4Extensions.FromHex(@"668800");
        public readonly Colour4 GreenDarker = Color4Extensions.FromHex(@"445500");

        public readonly Colour4 Sky = Color4Extensions.FromHex(@"6bb5ff");
        public readonly Colour4 GreySkyLighter = Color4Extensions.FromHex(@"c6e3f4");
        public readonly Colour4 GreySkyLight = Color4Extensions.FromHex(@"8ab3cc");
        public readonly Colour4 GreySky = Color4Extensions.FromHex(@"405461");
        public readonly Colour4 GreySkyDark = Color4Extensions.FromHex(@"303d47");
        public readonly Colour4 GreySkyDarker = Color4Extensions.FromHex(@"21272c");

        public readonly Colour4 Seafoam = Color4Extensions.FromHex(@"05ffa2");
        public readonly Colour4 GreySeafoamLighter = Color4Extensions.FromHex(@"9ebab1");
        public readonly Colour4 GreySeafoamLight = Color4Extensions.FromHex(@"4d7365");
        public readonly Colour4 GreySeafoam = Color4Extensions.FromHex(@"33413c");
        public readonly Colour4 GreySeafoamDark = Color4Extensions.FromHex(@"2c3532");
        public readonly Colour4 GreySeafoamDarker = Color4Extensions.FromHex(@"1e2422");

        public readonly Colour4 Cyan = Color4Extensions.FromHex(@"05f4fd");
        public readonly Colour4 GreyCyanLighter = Color4Extensions.FromHex(@"77b1b3");
        public readonly Colour4 GreyCyanLight = Color4Extensions.FromHex(@"436d6f");
        public readonly Colour4 GreyCyan = Color4Extensions.FromHex(@"293d3e");
        public readonly Colour4 GreyCyanDark = Color4Extensions.FromHex(@"243536");
        public readonly Colour4 GreyCyanDarker = Color4Extensions.FromHex(@"1e2929");

        public readonly Colour4 Lime = Color4Extensions.FromHex(@"82ff05");
        public readonly Colour4 GreyLimeLighter = Color4Extensions.FromHex(@"deff87");
        public readonly Colour4 GreyLimeLight = Color4Extensions.FromHex(@"657259");
        public readonly Colour4 GreyLime = Color4Extensions.FromHex(@"3f443a");
        public readonly Colour4 GreyLimeDark = Color4Extensions.FromHex(@"32352e");
        public readonly Colour4 GreyLimeDarker = Color4Extensions.FromHex(@"2e302b");

        public readonly Colour4 Violet = Color4Extensions.FromHex(@"bf04ff");
        public readonly Colour4 GreyVioletLighter = Color4Extensions.FromHex(@"ebb8fe");
        public readonly Colour4 GreyVioletLight = Color4Extensions.FromHex(@"685370");
        public readonly Colour4 GreyViolet = Color4Extensions.FromHex(@"46334d");
        public readonly Colour4 GreyVioletDark = Color4Extensions.FromHex(@"2c2230");
        public readonly Colour4 GreyVioletDarker = Color4Extensions.FromHex(@"201823");

        public readonly Colour4 Carmine = Color4Extensions.FromHex(@"ff0542");
        public readonly Colour4 GreyCarmineLighter = Color4Extensions.FromHex(@"deaab4");
        public readonly Colour4 GreyCarmineLight = Color4Extensions.FromHex(@"644f53");
        public readonly Colour4 GreyCarmine = Color4Extensions.FromHex(@"342b2d");
        public readonly Colour4 GreyCarmineDark = Color4Extensions.FromHex(@"302a2b");
        public readonly Colour4 GreyCarmineDarker = Color4Extensions.FromHex(@"241d1e");

        public readonly Colour4 Gray0 = Color4Extensions.FromHex(@"000");
        public readonly Colour4 Gray1 = Color4Extensions.FromHex(@"111");
        public readonly Colour4 Gray2 = Color4Extensions.FromHex(@"222");
        public readonly Colour4 Gray3 = Color4Extensions.FromHex(@"333");
        public readonly Colour4 Gray4 = Color4Extensions.FromHex(@"444");
        public readonly Colour4 Gray5 = Color4Extensions.FromHex(@"555");
        public readonly Colour4 Gray6 = Color4Extensions.FromHex(@"666");
        public readonly Colour4 Gray7 = Color4Extensions.FromHex(@"777");
        public readonly Colour4 Gray8 = Color4Extensions.FromHex(@"888");
        public readonly Colour4 Gray9 = Color4Extensions.FromHex(@"999");
        public readonly Colour4 GrayA = Color4Extensions.FromHex(@"aaa");
        public readonly Colour4 GrayB = Color4Extensions.FromHex(@"bbb");
        public readonly Colour4 GrayC = Color4Extensions.FromHex(@"ccc");
        public readonly Colour4 GrayD = Color4Extensions.FromHex(@"ddd");
        public readonly Colour4 GrayE = Color4Extensions.FromHex(@"eee");
        public readonly Colour4 GrayF = Color4Extensions.FromHex(@"fff");

        public readonly Colour4 RedLighter = Color4Extensions.FromHex(@"ffeded");
        public readonly Colour4 RedLight = Color4Extensions.FromHex(@"ed7787");
        public readonly Colour4 Red = Color4Extensions.FromHex(@"ed1121");
        public readonly Colour4 RedDark = Color4Extensions.FromHex(@"ba0011");
        public readonly Colour4 RedDarker = Color4Extensions.FromHex(@"870000");

        public readonly Colour4 ChatBlue = Color4Extensions.FromHex(@"17292e");

        public readonly Colour4 ContextMenuGray = Color4Extensions.FromHex(@"223034");
    }
}
