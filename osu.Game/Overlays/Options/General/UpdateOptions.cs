using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.General
{
    public class UpdateOptions : OptionsSubsection
    {
        protected override string Header => "Updates";

        [BackgroundDependencyLoader]
        private void load(BasicStorage storage)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                new SpriteText { Text = "Your osu! is up to date" }, // TODO: map this to reality
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Open osu! folder",
                    Action = () => storage.OpenInNativeExplorer(),
                }
            };
        }
    }
}

