// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics
{
    public static class OsuIcon
    {
        #region Legacy spritesheet-based icons

        private static IconUsage get(int icon) => new IconUsage((char)icon, @"osuFont");

        // ruleset icons in circles
        public static IconUsage RulesetOsu => get(0xe000);
        public static IconUsage RulesetMania => get(0xe001);
        public static IconUsage RulesetCatch => get(0xe002);
        public static IconUsage RulesetTaiko => get(0xe003);

        // ruleset icons without circles
        public static IconUsage FilledCircle => get(0xe004);
        public static IconUsage CrossCircle => get(0xe005);
        public static IconUsage Logo => get(0xe006);
        public static IconUsage ChevronDownCircle => get(0xe007);
        public static IconUsage EditCircle => get(0xe033);
        public static IconUsage LeftCircle => get(0xe034);
        public static IconUsage RightCircle => get(0xe035);
        public static IconUsage Charts => get(0xe036);
        public static IconUsage Solo => get(0xe037);
        public static IconUsage Multi => get(0xe038);
        public static IconUsage Gear => get(0xe039);

        // misc icons
        public static IconUsage Bat => get(0xe008);
        public static IconUsage Bubble => get(0xe009);
        public static IconUsage BubblePop => get(0xe02e);
        public static IconUsage Dice => get(0xe011);
        public static IconUsage Heart => get(0xe02f);
        public static IconUsage HeartBreak => get(0xe030);
        public static IconUsage Hot => get(0xe031);
        public static IconUsage ListSearch => get(0xe032);

        //osu! playstyles
        public static IconUsage PlayStyleTablet => get(0xe02a);
        public static IconUsage PlayStyleMouse => get(0xe029);
        public static IconUsage PlayStyleKeyboard => get(0xe02b);
        public static IconUsage PlayStyleTouch => get(0xe02c);

        // osu! difficulties
        public static IconUsage EasyOsu => get(0xe015);
        public static IconUsage NormalOsu => get(0xe016);
        public static IconUsage HardOsu => get(0xe017);
        public static IconUsage InsaneOsu => get(0xe018);
        public static IconUsage ExpertOsu => get(0xe019);

        // taiko difficulties
        public static IconUsage EasyTaiko => get(0xe01a);
        public static IconUsage NormalTaiko => get(0xe01b);
        public static IconUsage HardTaiko => get(0xe01c);
        public static IconUsage InsaneTaiko => get(0xe01d);
        public static IconUsage ExpertTaiko => get(0xe01e);

        // fruits difficulties
        public static IconUsage EasyFruits => get(0xe01f);
        public static IconUsage NormalFruits => get(0xe020);
        public static IconUsage HardFruits => get(0xe021);
        public static IconUsage InsaneFruits => get(0xe022);
        public static IconUsage ExpertFruits => get(0xe023);

        // mania difficulties
        public static IconUsage EasyMania => get(0xe024);
        public static IconUsage NormalMania => get(0xe025);
        public static IconUsage HardMania => get(0xe026);
        public static IconUsage InsaneMania => get(0xe027);
        public static IconUsage ExpertMania => get(0xe028);

        // mod icons
        public static IconUsage ModPerfect => get(0xe049);
        public static IconUsage ModAutopilot => get(0xe03a);
        public static IconUsage ModAuto => get(0xe03b);
        public static IconUsage ModCinema => get(0xe03c);
        public static IconUsage ModDoubleTime => get(0xe03d);
        public static IconUsage ModEasy => get(0xe03e);
        public static IconUsage ModFlashlight => get(0xe03f);
        public static IconUsage ModHalftime => get(0xe040);
        public static IconUsage ModHardRock => get(0xe041);
        public static IconUsage ModHidden => get(0xe042);
        public static IconUsage ModNightcore => get(0xe043);
        public static IconUsage ModNoFail => get(0xe044);
        public static IconUsage ModRelax => get(0xe045);
        public static IconUsage ModSpunOut => get(0xe046);
        public static IconUsage ModSuddenDeath => get(0xe047);
        public static IconUsage ModTarget => get(0xe048);

        // Use "Icons/BeatmapDetails/mod-icon" instead
        // public static IconUsage ModBg => Get(0xe04a);

        #endregion
    }
}
