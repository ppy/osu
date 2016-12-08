//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.General
{
    public class UpdateOptions : OptionsSubsection
    {
        protected override string Header => "Updates";

        [BackgroundDependencyLoader]
        private void load(BasicStorage storage, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new DropdownOption<ReleaseStream>
                {
                    LabelText = "Release stream",
                    Bindable = config.GetBindable<ReleaseStream>(OsuConfig.ReleaseStream),
                },
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

