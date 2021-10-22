namespace osu.Game.Screens.LLin.Plugins
{
    public abstract class LLinPluginProvider
    {
        /// <summary>
        /// 要提供的插件
        /// </summary>
        public abstract LLinPlugin CreatePlugin { get; }
    }
}
