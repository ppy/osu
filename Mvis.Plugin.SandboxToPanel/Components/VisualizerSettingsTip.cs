using Mvis.Plugin.RulesetPanel.Overlays;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Components
{
    public class VisualizerSettingsTip : SandboxOverlay
    {
        private readonly BindableBool showAgain = new BindableBool();

        protected override SandboxOverlayButton[] CreateButtons() => new[]
        {
            new SandboxOverlayButton("Got it")
            {
                ClickAction = onClick
            }
        };

        private SandboxCheckbox checkbox;

        protected override Drawable CreateContent() => new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                new TextFlowContainer(f =>
                {
                    f.Colour = Color4.Black;
                    f.Font = OsuFont.GetFont(size: 30, weight: FontWeight.Regular);
                })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    TextAnchor = Anchor.TopCentre,
                    Text = "To open visualizer settings move your cursor to the right side of the screen."
                },
                checkbox = new SandboxCheckbox("Don't show again")
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    BypassAutoSizeAxes = Axes.Y,
                    Y = 30
                }
            }
        };

        [Resolved]
        private SandboxConfigManager config { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            config.BindWith(SandboxSetting.ShowSettingsTip, showAgain);
        }

        private void onClick()
        {
            if (checkbox.Current.Value)
                showAgain.Value = false;

            Hide();
        }
    }
}
