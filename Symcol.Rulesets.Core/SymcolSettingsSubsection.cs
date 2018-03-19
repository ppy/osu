using osu.Framework.Allocation;
using osu.Game;
using osu.Game.Overlays.Settings;
using Symcol.Rulesets.Core.Wiki;
using osu.Game.Screens.Symcol;
using Symcol.Rulesets.Core.Multiplayer.Screens;
using osu.Framework.Platform;

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
            if (RulesetLobbyItem != null)
                RulesetMultiplayerSelection.LobbyItems.Add(RulesetLobbyItem);

            if (RulesetMultiplayerSelection == null)
                RulesetMultiplayerSelection = new RulesetMultiplayerSelection();
            SymcolMenu.RulesetMultiplayerScreen = RulesetMultiplayerSelection;
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
