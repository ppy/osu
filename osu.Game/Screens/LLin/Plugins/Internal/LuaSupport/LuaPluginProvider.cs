namespace osu.Game.Screens.LLin.Plugins.Internal.LuaSupport
{
    public class LuaPluginProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new LuaPlugin();
    }
}
