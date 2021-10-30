using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.Sandbox
{
    public class SandboxPanelProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new SandboxPanel();
    }
}
