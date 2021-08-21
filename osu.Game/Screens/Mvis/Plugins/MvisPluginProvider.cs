namespace osu.Game.Screens.Mvis.Plugins
{
    public abstract class MvisPluginProvider
    {
        /// <summary>
        /// 要提供的插件
        /// </summary>
        public abstract MvisPlugin CreatePlugin { get; }
    }
}
