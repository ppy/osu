using osu.Framework.Localisation;

namespace M.Resources.Localisation.Mvis
{
    public static class MvisGenericStrings
    {
        private const string prefix = @"M.Resources.Localisation.Mvis.GenericStrings";

        public static LocalisableString EnablePlugin => new TranslatableString(getKey(@"enable_plugin"), @"启用插件");

        public static LocalisableString DisablePlugin => new TranslatableString(getKey(@"disable_plugin"), @"禁用插件");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
