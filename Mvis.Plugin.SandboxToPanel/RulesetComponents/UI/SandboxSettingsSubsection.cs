using Mvis.Plugin.SandboxToPanel.RulesetComponents.Extensions;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Main;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI
{
    public partial class SandboxSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "Sandbox";

        [Resolved]
        private OsuGame game { get; set; }

        public SandboxSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "Open Main Screen",
                    Action = () =>
                    {
                        try
                        {
                            var screenStack = game.GetScreenStack();
                            if (!(screenStack.CurrentScreen is MainMenu))
                                return;

                            var settingOverlay = game.GetSettingsOverlay();
                            screenStack?.Push(new MainRulesetScreen());
                            settingOverlay?.Hide();
                        }
                        catch
                        {
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "(Available only in the Main Menu)"
                    }
                }
            };
        }
    }
}
