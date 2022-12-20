using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Overlays.Settings;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI
{
    public partial class SupportButton : CompositeDrawable
    {
        [Resolved]
        private GameHost host { get; set; }

        public SupportButton()
        {
            Width = 300;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "Support me on Patreon",
                    Action = clickAction
                }
            };
        }

        private void clickAction()
        {
            host.OpenUrlExternally($"https://www.patreon.com/evast");
        }
    }
}
