using osu.Framework.Allocation;
using osu.Game;
using osu.Game.Overlays.Settings;
using Symcol.Rulesets.Core.Wiki;
using osu.Game.Screens.Symcol;
using Symcol.Rulesets.Core.Multiplayer.Screens;
using osu.Framework.Platform;
using osu.Framework.Logging;

namespace Symcol.Rulesets.Core
{
    public abstract class SymcolSettingsSubsection : SettingsSubsection
    {
        public virtual WikiOverlay Wiki => null;

        public virtual RulesetLobbyItem RulesetLobbyItem => null;

        public static RulesetMultiplayerSelection RulesetMultiplayerSelection;

        public static SymcolConfigManager SymcolConfigManager;

        private OsuGame osu;

        public SymcolSettingsSubsection()
        {
            try
            {
                if (RulesetLobbyItem != null)
                    RulesetMultiplayerSelection.LobbyItems.Add(RulesetLobbyItem);

                if (RulesetMultiplayerSelection == null)
                    RulesetMultiplayerSelection = new RulesetMultiplayerSelection();

                SymcolMenu.RulesetMultiplayerScreen = RulesetMultiplayerSelection;
            }
            catch
            {
                Logger.Log("osu.Game mods not installed! Online Multiplayer will not be avalible without them. . .", LoggingTarget.Information, LogLevel.Important);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame osu, Storage storage)
        {
            this.osu = osu;

            if (SymcolConfigManager == null)
                SymcolConfigManager = new SymcolConfigManager(storage);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Wiki != null)
                osu.Add(Wiki);
        }
    }
}
