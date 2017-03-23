// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.General
{
    public class UpdateOptions : OptionsSubsection
    {
        protected override string Header => "Updates";

        [BackgroundDependencyLoader]
        private void load(Storage storage, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionEnumDropdown<ReleaseStream>
                {
                    LabelText = "Release stream",
                    Bindable = config.GetBindable<ReleaseStream>(OsuConfig.ReleaseStream),
                },
                new OptionLabel { Text = "Your osu! is up to date" }, // TODO: map this to reality
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

