// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics
{
    public static class OsuIcon
    {
        public static IconUsage Get(int icon) => new IconUsage((char)icon, "osuFont");

        // ruleset icons in circles
        public static IconUsage RulesetOsu => Get(0xe000);
        public static IconUsage RulesetMania => Get(0xe001);
        public static IconUsage RulesetCatch => Get(0xe002);
        public static IconUsage RulesetTaiko => Get(0xe003);

        // ruleset icons without circles
        public static IconUsage FilledCircle => Get(0xe004);
        public static IconUsage CrossCircle => Get(0xe005);
        public static IconUsage Logo => Get(0xe006);
        public static IconUsage ChevronDownCircle => Get(0xe007);
        public static IconUsage EditCircle => Get(0xe033);
        public static IconUsage LeftCircle => Get(0xe034);
        public static IconUsage RightCircle => Get(0xe035);
        public static IconUsage Charts => Get(0xe036);
        public static IconUsage Solo => Get(0xe037);
        public static IconUsage Multi => Get(0xe038);
        public static IconUsage Gear => Get(0xe039);

        // misc icons
        public static IconUsage Bat => Get(0xe008);
        public static IconUsage Bubble => Get(0xe009);
        public static IconUsage BubblePop => Get(0xe02e);
        public static IconUsage Dice => Get(0xe011);
        public static IconUsage Heart => Get(0xe02f);
        public static IconUsage HeartBreak => Get(0xe030);
        public static IconUsage Hot => Get(0xe031);
        public static IconUsage ListSearch => Get(0xe032);

        //osu! playstyles
        public static IconUsage PlayStyleTablet => Get(0xe02a);
        public static IconUsage PlayStyleMouse => Get(0xe029);
        public static IconUsage PlayStyleKeyboard => Get(0xe02b);
        public static IconUsage PlayStyleTouch => Get(0xe02c);

        // osu! difficulties
        public static IconUsage EasyOsu => Get(0xe015);
        public static IconUsage NormalOsu => Get(0xe016);
        public static IconUsage HardOsu => Get(0xe017);
        public static IconUsage InsaneOsu => Get(0xe018);
        public static IconUsage ExpertOsu => Get(0xe019);

        // taiko difficulties
        public static IconUsage EasyTaiko => Get(0xe01a);
        public static IconUsage NormalTaiko => Get(0xe01b);
        public static IconUsage HardTaiko => Get(0xe01c);
        public static IconUsage InsaneTaiko => Get(0xe01d);
        public static IconUsage ExpertTaiko => Get(0xe01e);

        // fruits difficulties
        public static IconUsage EasyFruits => Get(0xe01f);
        public static IconUsage NormalFruits => Get(0xe020);
        public static IconUsage HardFruits => Get(0xe021);
        public static IconUsage InsaneFruits => Get(0xe022);
        public static IconUsage ExpertFruits => Get(0xe023);

        // mania difficulties
        public static IconUsage EasyMania => Get(0xe024);
        public static IconUsage NormalMania => Get(0xe025);
        public static IconUsage HardMania => Get(0xe026);
        public static IconUsage InsaneMania => Get(0xe027);
        public static IconUsage ExpertMania => Get(0xe028);

        // mod icons
        public static IconUsage ModPerfect => Get(0xe049);
        public static IconUsage ModAutopilot => Get(0xe03a);
        public static IconUsage ModAuto => Get(0xe03b);
        public static IconUsage ModCinema => Get(0xe03c);
        public static IconUsage ModDoubleTime => Get(0xe03d);
        public static IconUsage ModEasy => Get(0xe03e);
        public static IconUsage ModFlashlight => Get(0xe03f);
        public static IconUsage ModHalftime => Get(0xe040);
        public static IconUsage ModHardRock => Get(0xe041);
        public static IconUsage ModHidden => Get(0xe042);
        public static IconUsage ModNightcore => Get(0xe043);
        public static IconUsage ModNoFail => Get(0xe044);
        public static IconUsage ModRelax => Get(0xe045);
        public static IconUsage ModSpunOut => Get(0xe046);
        public static IconUsage ModSuddenDeath => Get(0xe047);
        public static IconUsage ModTarget => Get(0xe048);

        // Use "Icons/BeatmapDetails/mod-icon" instead
        // public static IconUsage ModBg => Get(0xe04a);
    }
}
