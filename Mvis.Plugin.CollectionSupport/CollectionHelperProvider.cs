using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.CollectionSupport
{
    public class CollectionHelperProvider : MvisPluginProvider
    {
        public override MvisPlugin CreatePlugin => new CollectionHelper();
    }
}
