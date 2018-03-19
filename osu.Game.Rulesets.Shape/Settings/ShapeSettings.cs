using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Overlays.Settings;
using Symcol.Rulesets.Core;
using Symcol.Rulesets.Core.Wiki;
using osu.Game.Rulesets.Shape.Wiki;
using Symcol.Rulesets.Core.Multiplayer.Screens;
using osu.Game.Rulesets.Shape.Multi;

namespace osu.Game.Rulesets.Shape.Settings
{
    public class ShapeSettings : SymcolSettingsSubsection
    {
        protected override string Header => "shape!";

        public override WikiOverlay Wiki => shapeWiki;

        private readonly ShapeWikiOverlay shapeWiki = new ShapeWikiOverlay();

        public override RulesetLobbyItem RulesetLobbyItem => shapeLobby;

        private readonly ShapeLobbyItem shapeLobby = new ShapeLobbyItem();

        public static ShapeConfigManager ShapeConfigManager;

        [BackgroundDependencyLoader]
        private void load(GameHost host, Storage storage)
        {
            ShapeConfigManager = new ShapeConfigManager(host.Storage);

            Storage skinsStorage = storage.GetStorageForDirectory("Skins");

            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "Open In-game Wiki",
                    Action = shapeWiki.Show
                }
            };
        }
    }
}
