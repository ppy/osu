using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.CollectionSupport
{
    public class CollectionHelperProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new CollectionHelper();
    }
}
