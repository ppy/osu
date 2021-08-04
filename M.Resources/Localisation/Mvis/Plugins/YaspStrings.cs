using osu.Framework.Localisation;

namespace M.Resources.Localisation.Mvis.Plugins
{
    public static class YaspStrings
    {
        private const string prefix = @"M.Resources.Localisation.Mvis.Plugins.YaspStrings";

        public static LocalisableString Scale => new TranslatableString(getKey(@"scale"), @"缩放");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
