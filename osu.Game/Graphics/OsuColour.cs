// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Graphics
{
    public class OsuColour
    {
        public static Color4 Gray(float amt) => new Color4(amt, amt, amt, 1f);
        public static Color4 Gray(byte amt) => new Color4(amt, amt, amt, 255);

        /// <summary>
        /// Retrieves the colour for a <see cref="DifficultyRating"/>.
        /// </summary>
        /// <remarks>
        /// Sourced from the @diff-{rating} variables in https://github.com/ppy/osu-web/blob/71fbab8936d79a7929d13854f5e854b4f383b236/resources/assets/less/variables.less.
        /// </remarks>
        public Color4 ForDifficultyRating(DifficultyRating difficulty, bool useLighterColour = false)
        {
            switch (difficulty)
            {
                case DifficultyRating.Easy:
                    return Color4Extensions.FromHex("4ebfff");

                case DifficultyRating.Normal:
                    return Color4Extensions.FromHex("66ff91");

                case DifficultyRating.Hard:
                    return Color4Extensions.FromHex("f7e85d");

                case DifficultyRating.Insane:
                    return Color4Extensions.FromHex("ff7e68");

                case DifficultyRating.Expert:
                    return Color4Extensions.FromHex("fe3c71");

                case DifficultyRating.ExpertPlus:
                    return Color4Extensions.FromHex("6662dd");

                default:
                    throw new ArgumentOutOfRangeException(nameof(difficulty));
            }
        }

        public Color4 ForStarDifficulty(double starDifficulty) => ColourUtils.SampleFromLinearGradient(new[]
        {
            (0.1f, Color4Extensions.FromHex("aaaaaa")),
            (0.1f, Color4Extensions.FromHex("4290fb")),
            (1.25f, Color4Extensions.FromHex("4fc0ff")),
            (2.0f, Color4Extensions.FromHex("4fffd5")),
            (2.5f, Color4Extensions.FromHex("7cff4f")),
            (3.3f, Color4Extensions.FromHex("f6f05c")),
            (4.2f, Color4Extensions.FromHex("ff8068")),
            (4.9f, Color4Extensions.FromHex("ff4e6f")),
            (5.8f, Color4Extensions.FromHex("c645b8")),
            (6.7f, Color4Extensions.FromHex("6563de")),
            (7.7f, Color4Extensions.FromHex("18158e")),
            (9.0f, Color4.Black),
        }, (float)Math.Round(starDifficulty, 2, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Retrieves the colour for a <see cref="ScoreRank"/>.
        /// </summary>
        public static Color4 ForRank(ScoreRank rank)
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
        public Color4 ForHitResult(HitResult judgement)
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
                    return Color4.White;
            }
        }

        /// <summary>
        /// Retrieves a colour for the given <see cref="BeatmapSetOnlineStatus"/>.
        /// A <see langword="null"/> value indicates that a "background" shade from the local <see cref="OverlayColourProvider"/>
        /// (or another fallback colour) should be used.
        /// </summary>
        /// <remarks>
        /// Sourced from web: https://github.com/ppy/osu-web/blob/007eebb1916ed5cb6a7866d82d8011b1060a945e/resources/assets/less/layout.less#L36-L50
        /// </remarks>
        public static Color4? ForBeatmapSetOnlineStatus(BeatmapSetOnlineStatus status)
        {
            switch (status)
            {
                case BeatmapSetOnlineStatus.Ranked:
                case BeatmapSetOnlineStatus.Approved:
                    return Color4Extensions.FromHex(@"b3ff66");

                case BeatmapSetOnlineStatus.Loved:
                    return Color4Extensions.FromHex(@"ff66ab");

                case BeatmapSetOnlineStatus.Qualified:
                    return Color4Extensions.FromHex(@"66ccff");

                case BeatmapSetOnlineStatus.Pending:
                    return Color4Extensions.FromHex(@"ffd966");

                case BeatmapSetOnlineStatus.WIP:
                    return Color4Extensions.FromHex(@"ff9966");

                case BeatmapSetOnlineStatus.Graveyard:
                    return Color4.Black;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a foreground text colour that is supposed to contrast well with
        /// the supplied <paramref name="backgroundColour"/>.
        /// </summary>
        public static Color4 ForegroundTextColourFor(Color4 backgroundColour)
        {
            // formula taken from the RGB->YIQ conversions: https://en.wikipedia.org/wiki/YIQ
            // brightness here is equivalent to the Y component in the above colour model, which is a rough estimate of lightness.
            float brightness = 0.299f * backgroundColour.R + 0.587f * backgroundColour.G + 0.114f * backgroundColour.B;
            return Gray(brightness > 0.5f ? 0.2f : 0.9f);
        }

        public readonly Color4 TeamColourRed = Color4Extensions.FromHex("#AA1414");
        public readonly Color4 TeamColourBlue = Color4Extensions.FromHex("#1462AA");

        // See https://github.com/ppy/osu-web/blob/master/resources/assets/less/colors.less
        public readonly Color4 PurpleLighter = Color4Extensions.FromHex(@"eeeeff");
        public readonly Color4 PurpleLight = Color4Extensions.FromHex(@"aa88ff");
        public readonly Color4 PurpleLightAlternative = Color4Extensions.FromHex(@"cba4da");
        public readonly Color4 Purple = Color4Extensions.FromHex(@"8866ee");
        public readonly Color4 PurpleDark = Color4Extensions.FromHex(@"6644cc");
        public readonly Color4 PurpleDarkAlternative = Color4Extensions.FromHex(@"312436");
        public readonly Color4 PurpleDarker = Color4Extensions.FromHex(@"441188");

        public readonly Color4 PinkLighter = Color4Extensions.FromHex(@"ffddee");
        public readonly Color4 PinkLight = Color4Extensions.FromHex(@"ff99cc");
        public readonly Color4 Pink = Color4Extensions.FromHex(@"ff66aa");
        public readonly Color4 PinkDark = Color4Extensions.FromHex(@"cc5288");
        public readonly Color4 PinkDarker = Color4Extensions.FromHex(@"bb1177");

        public readonly Color4 BlueLighter = Color4Extensions.FromHex(@"ddffff");
        public readonly Color4 BlueLight = Color4Extensions.FromHex(@"99eeff");
        public readonly Color4 Blue = Color4Extensions.FromHex(@"66ccff");
        public readonly Color4 BlueDark = Color4Extensions.FromHex(@"44aadd");
        public readonly Color4 BlueDarker = Color4Extensions.FromHex(@"2299bb");

        public readonly Color4 YellowLighter = Color4Extensions.FromHex(@"ffffdd");
        public readonly Color4 YellowLight = Color4Extensions.FromHex(@"ffdd55");
        public readonly Color4 Yellow = Color4Extensions.FromHex(@"ffcc22");
        public readonly Color4 YellowDark = Color4Extensions.FromHex(@"eeaa00");
        public readonly Color4 YellowDarker = Color4Extensions.FromHex(@"cc6600");

        public readonly Color4 GreenLighter = Color4Extensions.FromHex(@"eeffcc");
        public readonly Color4 GreenLight = Color4Extensions.FromHex(@"b3d944");
        public readonly Color4 Green = Color4Extensions.FromHex(@"88b300");
        public readonly Color4 GreenDark = Color4Extensions.FromHex(@"668800");
        public readonly Color4 GreenDarker = Color4Extensions.FromHex(@"445500");

        public readonly Color4 Sky = Color4Extensions.FromHex(@"6bb5ff");
        public readonly Color4 GreySkyLighter = Color4Extensions.FromHex(@"c6e3f4");
        public readonly Color4 GreySkyLight = Color4Extensions.FromHex(@"8ab3cc");
        public readonly Color4 GreySky = Color4Extensions.FromHex(@"405461");
        public readonly Color4 GreySkyDark = Color4Extensions.FromHex(@"303d47");
        public readonly Color4 GreySkyDarker = Color4Extensions.FromHex(@"21272c");

        public readonly Color4 Seafoam = Color4Extensions.FromHex(@"05ffa2");
        public readonly Color4 GreySeafoamLighter = Color4Extensions.FromHex(@"9ebab1");
        public readonly Color4 GreySeafoamLight = Color4Extensions.FromHex(@"4d7365");
        public readonly Color4 GreySeafoam = Color4Extensions.FromHex(@"33413c");
        public readonly Color4 GreySeafoamDark = Color4Extensions.FromHex(@"2c3532");
        public readonly Color4 GreySeafoamDarker = Color4Extensions.FromHex(@"1e2422");

        public readonly Color4 Cyan = Color4Extensions.FromHex(@"05f4fd");
        public readonly Color4 GreyCyanLighter = Color4Extensions.FromHex(@"77b1b3");
        public readonly Color4 GreyCyanLight = Color4Extensions.FromHex(@"436d6f");
        public readonly Color4 GreyCyan = Color4Extensions.FromHex(@"293d3e");
        public readonly Color4 GreyCyanDark = Color4Extensions.FromHex(@"243536");
        public readonly Color4 GreyCyanDarker = Color4Extensions.FromHex(@"1e2929");

        public readonly Color4 Lime = Color4Extensions.FromHex(@"82ff05");
        public readonly Color4 GreyLimeLighter = Color4Extensions.FromHex(@"deff87");
        public readonly Color4 GreyLimeLight = Color4Extensions.FromHex(@"657259");
        public readonly Color4 GreyLime = Color4Extensions.FromHex(@"3f443a");
        public readonly Color4 GreyLimeDark = Color4Extensions.FromHex(@"32352e");
        public readonly Color4 GreyLimeDarker = Color4Extensions.FromHex(@"2e302b");

        public readonly Color4 Violet = Color4Extensions.FromHex(@"bf04ff");
        public readonly Color4 GreyVioletLighter = Color4Extensions.FromHex(@"ebb8fe");
        public readonly Color4 GreyVioletLight = Color4Extensions.FromHex(@"685370");
        public readonly Color4 GreyViolet = Color4Extensions.FromHex(@"46334d");
        public readonly Color4 GreyVioletDark = Color4Extensions.FromHex(@"2c2230");
        public readonly Color4 GreyVioletDarker = Color4Extensions.FromHex(@"201823");

        public readonly Color4 Carmine = Color4Extensions.FromHex(@"ff0542");
        public readonly Color4 GreyCarmineLighter = Color4Extensions.FromHex(@"deaab4");
        public readonly Color4 GreyCarmineLight = Color4Extensions.FromHex(@"644f53");
        public readonly Color4 GreyCarmine = Color4Extensions.FromHex(@"342b2d");
        public readonly Color4 GreyCarmineDark = Color4Extensions.FromHex(@"302a2b");
        public readonly Color4 GreyCarmineDarker = Color4Extensions.FromHex(@"241d1e");

        public readonly Color4 Gray0 = Color4Extensions.FromHex(@"000");
        public readonly Color4 Gray1 = Color4Extensions.FromHex(@"111");
        public readonly Color4 Gray2 = Color4Extensions.FromHex(@"222");
        public readonly Color4 Gray3 = Color4Extensions.FromHex(@"333");
        public readonly Color4 Gray4 = Color4Extensions.FromHex(@"444");
        public readonly Color4 Gray5 = Color4Extensions.FromHex(@"555");
        public readonly Color4 Gray6 = Color4Extensions.FromHex(@"666");
        public readonly Color4 Gray7 = Color4Extensions.FromHex(@"777");
        public readonly Color4 Gray8 = Color4Extensions.FromHex(@"888");
        public readonly Color4 Gray9 = Color4Extensions.FromHex(@"999");
        public readonly Color4 GrayA = Color4Extensions.FromHex(@"aaa");
        public readonly Color4 GrayB = Color4Extensions.FromHex(@"bbb");
        public readonly Color4 GrayC = Color4Extensions.FromHex(@"ccc");
        public readonly Color4 GrayD = Color4Extensions.FromHex(@"ddd");
        public readonly Color4 GrayE = Color4Extensions.FromHex(@"eee");
        public readonly Color4 GrayF = Color4Extensions.FromHex(@"fff");

        /// <summary>
        /// Equivalent to <see cref="OverlayColourProvider.Pink"/>'s <see cref="OverlayColourProvider.Colour3"/>.
        /// </summary>
        public readonly Color4 Pink3 = Color4Extensions.FromHex(@"cc3378");

        /// <summary>
        /// Equivalent to <see cref="OverlayColourProvider.Blue"/>'s <see cref="OverlayColourProvider.Colour3"/>.
        /// </summary>
        public readonly Color4 Blue3 = Color4Extensions.FromHex(@"3399cc");

        public readonly Color4 Lime0 = Color4Extensions.FromHex(@"ccff99");

        /// <summary>
        /// Equivalent to <see cref="OverlayColourProvider.Lime"/>'s <see cref="OverlayColourProvider.Colour1"/>.
        /// </summary>
        public readonly Color4 Lime1 = Color4Extensions.FromHex(@"b2ff66");

        /// <summary>
        /// Equivalent to <see cref="OverlayColourProvider.Lime"/>'s <see cref="OverlayColourProvider.Colour3"/>.
        /// </summary>
        public readonly Color4 Lime3 = Color4Extensions.FromHex(@"7fcc33");

        /// <summary>
        /// Equivalent to <see cref="OverlayColourProvider.Orange"/>'s <see cref="OverlayColourProvider.Colour1"/>.
        /// </summary>
        public readonly Color4 Orange1 = Color4Extensions.FromHex(@"ffd966");

        // Content Background
        public readonly Color4 B5 = Color4Extensions.FromHex(@"222a28");

        public readonly Color4 RedLighter = Color4Extensions.FromHex(@"ffeded");
        public readonly Color4 RedLight = Color4Extensions.FromHex(@"ed7787");
        public readonly Color4 Red = Color4Extensions.FromHex(@"ed1121");
        public readonly Color4 RedDark = Color4Extensions.FromHex(@"ba0011");
        public readonly Color4 RedDarker = Color4Extensions.FromHex(@"870000");

        public readonly Color4 ChatBlue = Color4Extensions.FromHex(@"17292e");

        public readonly Color4 ContextMenuGray = Color4Extensions.FromHex(@"223034");
    }
}
