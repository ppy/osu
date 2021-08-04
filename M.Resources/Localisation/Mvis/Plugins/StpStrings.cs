using osu.Framework.Localisation;

namespace M.Resources.Localisation.Mvis.Plugins
{
    //Sandbox to panel Strings
    public class StpStrings
    {
        private const string prefix = @"M.Resources.Localisation.Mvis.Plugins.StpStrings";

        public static LocalisableString AlphaOnIdle => new TranslatableString(getKey(@"alpha_on_idle"), @"空闲时的不透明度");

        public static LocalisableString ShowParticles => new TranslatableString(getKey(@"show_particles"), @"显示粒子");

        public static LocalisableString ParticleCount => new TranslatableString(getKey(@"particle_count"), @"粒子数");

        public static LocalisableString ShowBeatmapInfo => new TranslatableString(getKey(@"show_beatmap_info"), @"显示谱面信息");

        public static LocalisableString VisualizerLayoutType => new TranslatableString(getKey(@"layout_type"), @"界面类型");

        public static LocalisableString Radius => new TranslatableString(getKey(@"radius"), @"半径");

        public static LocalisableString BarType => new TranslatableString(getKey(@"bar_type"), @"频谱类型");

        public static LocalisableString BarWidth => new TranslatableString(getKey(@"bar_width"), @"频谱宽度");

        public static LocalisableString BarCount => new TranslatableString(getKey(@"bar_count"), @"频谱数量");

        public static LocalisableString Rotation => new TranslatableString(getKey(@"rotation"), @"旋转角度");

        public static LocalisableString DecayTime => new TranslatableString(getKey(@"decay"), @"复原时间");

        public static LocalisableString HeightMultiplier => new TranslatableString(getKey(@"height_multiplier"), @"高度倍率");

        public static LocalisableString Symmetry => new TranslatableString(getKey(@"symmetry"), @"对称");

        public static LocalisableString Smoothness => new TranslatableString(getKey(@"smoothness"), @"平滑度");

        public static LocalisableString VisualizerAmount => new TranslatableString(getKey(@"visualizer_amount"), @"分段数");

        public static LocalisableString BarsPerVisual => new TranslatableString(getKey(@"bars_per_visual"), @"频谱密度");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
