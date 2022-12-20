using Mvis.Plugin.SandboxToPanel.RulesetComponents.Extensions;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Main.Components;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Numbers;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Rulesets.UI;
using osu.Game.Screens;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Main
{
    public partial class MainRulesetScreen : SandboxScreen
    {
        public MainRulesetScreen()
        {
            InternalChildren = new Drawable[]
            {
                new SandboxButtonSystem
                {
                    Buttons = new[]
                    {
                        new Components.SandboxPanel("Visualizer", "Vis") { Action = () => this.Push(new VisualizerScreen()) },
                        new Components.SandboxPanel("2048", "Numbers") { Action = () => this.Push(new NumbersScreen()) },
                        new Components.SandboxPanel("FlappyDon", "Flappy", new Creator("https://github.com/TimOliver", "Tim Oliver")) { Action = () => this.Push(new FlappyDonScreen()) }
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Bottom = 30 },
                    Children = new Drawable[]
                    {
                        new CheckSandboxUpdatesButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new SupportButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                }
            };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var baseDependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            return new OsuScreenDependencies(false, new DrawableRulesetDependencies(baseDependencies.GetRuleset(), baseDependencies));
        }
    }
}
