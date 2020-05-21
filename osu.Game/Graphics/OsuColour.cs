// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
                    return Colour4.FromHex(@"de31ae");

                case ScoreRank.SH:
                case ScoreRank.S:
                    return Colour4.FromHex(@"02b5c3");

                case ScoreRank.A:
                    return Colour4.FromHex(@"88da20");

                case ScoreRank.B:
                    return Colour4.FromHex(@"e3b130");

                case ScoreRank.C:
                    return Colour4.FromHex(@"ff8e5d");

                default:
                    return Colour4.FromHex(@"ff5a5a");
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
        public readonly Colour4 PurpleLighter = Colour4.FromHex(@"eeeeff");
        public readonly Colour4 PurpleLight = Colour4.FromHex(@"aa88ff");
        public readonly Colour4 PurpleLightAlternative = Colour4.FromHex(@"cba4da");
        public readonly Colour4 Purple = Colour4.FromHex(@"8866ee");
        public readonly Colour4 PurpleDark = Colour4.FromHex(@"6644cc");
        public readonly Colour4 PurpleDarkAlternative = Colour4.FromHex(@"312436");
        public readonly Colour4 PurpleDarker = Colour4.FromHex(@"441188");

        public readonly Colour4 PinkLighter = Colour4.FromHex(@"ffddee");
        public readonly Colour4 PinkLight = Colour4.FromHex(@"ff99cc");
        public readonly Colour4 Pink = Colour4.FromHex(@"ff66aa");
        public readonly Colour4 PinkDark = Colour4.FromHex(@"cc5288");
        public readonly Colour4 PinkDarker = Colour4.FromHex(@"bb1177");

        public readonly Colour4 BlueLighter = Colour4.FromHex(@"ddffff");
        public readonly Colour4 BlueLight = Colour4.FromHex(@"99eeff");
        public readonly Colour4 Blue = Colour4.FromHex(@"66ccff");
        public readonly Colour4 BlueDark = Colour4.FromHex(@"44aadd");
        public readonly Colour4 BlueDarker = Colour4.FromHex(@"2299bb");

        public readonly Colour4 YellowLighter = Colour4.FromHex(@"ffffdd");
        public readonly Colour4 YellowLight = Colour4.FromHex(@"ffdd55");
        public readonly Colour4 Yellow = Colour4.FromHex(@"ffcc22");
        public readonly Colour4 YellowDark = Colour4.FromHex(@"eeaa00");
        public readonly Colour4 YellowDarker = Colour4.FromHex(@"cc6600");

        public readonly Colour4 GreenLighter = Colour4.FromHex(@"eeffcc");
        public readonly Colour4 GreenLight = Colour4.FromHex(@"b3d944");
        public readonly Colour4 Green = Colour4.FromHex(@"88b300");
        public readonly Colour4 GreenDark = Colour4.FromHex(@"668800");
        public readonly Colour4 GreenDarker = Colour4.FromHex(@"445500");

        public readonly Colour4 Sky = Colour4.FromHex(@"6bb5ff");
        public readonly Colour4 GreySkyLighter = Colour4.FromHex(@"c6e3f4");
        public readonly Colour4 GreySkyLight = Colour4.FromHex(@"8ab3cc");
        public readonly Colour4 GreySky = Colour4.FromHex(@"405461");
        public readonly Colour4 GreySkyDark = Colour4.FromHex(@"303d47");
        public readonly Colour4 GreySkyDarker = Colour4.FromHex(@"21272c");

        public readonly Colour4 Seafoam = Colour4.FromHex(@"05ffa2");
        public readonly Colour4 GreySeafoamLighter = Colour4.FromHex(@"9ebab1");
        public readonly Colour4 GreySeafoamLight = Colour4.FromHex(@"4d7365");
        public readonly Colour4 GreySeafoam = Colour4.FromHex(@"33413c");
        public readonly Colour4 GreySeafoamDark = Colour4.FromHex(@"2c3532");
        public readonly Colour4 GreySeafoamDarker = Colour4.FromHex(@"1e2422");

        public readonly Colour4 Cyan = Colour4.FromHex(@"05f4fd");
        public readonly Colour4 GreyCyanLighter = Colour4.FromHex(@"77b1b3");
        public readonly Colour4 GreyCyanLight = Colour4.FromHex(@"436d6f");
        public readonly Colour4 GreyCyan = Colour4.FromHex(@"293d3e");
        public readonly Colour4 GreyCyanDark = Colour4.FromHex(@"243536");
        public readonly Colour4 GreyCyanDarker = Colour4.FromHex(@"1e2929");

        public readonly Colour4 Lime = Colour4.FromHex(@"82ff05");
        public readonly Colour4 GreyLimeLighter = Colour4.FromHex(@"deff87");
        public readonly Colour4 GreyLimeLight = Colour4.FromHex(@"657259");
        public readonly Colour4 GreyLime = Colour4.FromHex(@"3f443a");
        public readonly Colour4 GreyLimeDark = Colour4.FromHex(@"32352e");
        public readonly Colour4 GreyLimeDarker = Colour4.FromHex(@"2e302b");

        public readonly Colour4 Violet = Colour4.FromHex(@"bf04ff");
        public readonly Colour4 GreyVioletLighter = Colour4.FromHex(@"ebb8fe");
        public readonly Colour4 GreyVioletLight = Colour4.FromHex(@"685370");
        public readonly Colour4 GreyViolet = Colour4.FromHex(@"46334d");
        public readonly Colour4 GreyVioletDark = Colour4.FromHex(@"2c2230");
        public readonly Colour4 GreyVioletDarker = Colour4.FromHex(@"201823");

        public readonly Colour4 Carmine = Colour4.FromHex(@"ff0542");
        public readonly Colour4 GreyCarmineLighter = Colour4.FromHex(@"deaab4");
        public readonly Colour4 GreyCarmineLight = Colour4.FromHex(@"644f53");
        public readonly Colour4 GreyCarmine = Colour4.FromHex(@"342b2d");
        public readonly Colour4 GreyCarmineDark = Colour4.FromHex(@"302a2b");
        public readonly Colour4 GreyCarmineDarker = Colour4.FromHex(@"241d1e");

        public readonly Colour4 Gray0 = Colour4.FromHex(@"000");
        public readonly Colour4 Gray1 = Colour4.FromHex(@"111");
        public readonly Colour4 Gray2 = Colour4.FromHex(@"222");
        public readonly Colour4 Gray3 = Colour4.FromHex(@"333");
        public readonly Colour4 Gray4 = Colour4.FromHex(@"444");
        public readonly Colour4 Gray5 = Colour4.FromHex(@"555");
        public readonly Colour4 Gray6 = Colour4.FromHex(@"666");
        public readonly Colour4 Gray7 = Colour4.FromHex(@"777");
        public readonly Colour4 Gray8 = Colour4.FromHex(@"888");
        public readonly Colour4 Gray9 = Colour4.FromHex(@"999");
        public readonly Colour4 GrayA = Colour4.FromHex(@"aaa");
        public readonly Colour4 GrayB = Colour4.FromHex(@"bbb");
        public readonly Colour4 GrayC = Colour4.FromHex(@"ccc");
        public readonly Colour4 GrayD = Colour4.FromHex(@"ddd");
        public readonly Colour4 GrayE = Colour4.FromHex(@"eee");
        public readonly Colour4 GrayF = Colour4.FromHex(@"fff");

        public readonly Colour4 RedLighter = Colour4.FromHex(@"ffeded");
        public readonly Colour4 RedLight = Colour4.FromHex(@"ed7787");
        public readonly Colour4 Red = Colour4.FromHex(@"ed1121");
        public readonly Colour4 RedDark = Colour4.FromHex(@"ba0011");
        public readonly Colour4 RedDarker = Colour4.FromHex(@"870000");

        public readonly Colour4 ChatBlue = Colour4.FromHex(@"17292e");

        public readonly Colour4 ContextMenuGray = Colour4.FromHex(@"223034");
    }
}
