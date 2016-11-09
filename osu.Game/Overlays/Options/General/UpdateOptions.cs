using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.General
{
    public class UpdateOptions : OptionsSubsection
    {
        private BasicStorage storage;
        protected override string Header => "Updates";

        public UpdateOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                new SpriteText { Text = "Your osu! is up to date" }, // TODO: map this to reality
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Open osu! folder",
                    Action = () => storage?.OpenInNativeExplorer(),
                }
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            this.storage = game.Host.Storage;
        }
    }
}

