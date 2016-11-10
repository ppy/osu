using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class SongSelectGraphicsOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";
        
        [Initializer]
        private void Load(OsuConfigManager config)
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