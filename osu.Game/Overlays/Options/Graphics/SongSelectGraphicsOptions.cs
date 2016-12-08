//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class SongSelectGraphicsOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";
        
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new CheckBoxOption
                {
                    LabelText = "Show thumbnails",
                    Bindable = config.GetBindable<bool>(OsuConfig.SongSelectThumbnails)
                }
            };
        }
    }
}