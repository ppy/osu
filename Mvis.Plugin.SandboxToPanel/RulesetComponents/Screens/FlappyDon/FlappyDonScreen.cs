using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon.Components;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon
{
    public partial class FlappyDonScreen : SandboxScreen
    {
        public FlappyDonScreen()
        {
            AddInternal(new FlappyDonGame());
        }
    }
}
