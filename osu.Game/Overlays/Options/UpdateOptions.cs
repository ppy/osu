using System;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;

namespace osu.Game.Overlays.Options
{
    public class UpdateOptions : OptionsSubsection
    {
        public UpdateOptions(BasicStorage storage)
        {
            Header = "Updates";
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                new SpriteText { Text = "Your osu! is up to date" }, // TODO: map this to reality
                new Button
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Open osu! folder",
                    Action = storage.OpenInNativeExplorer,
                }
            };
        }
    }
}

