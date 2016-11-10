using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Input
{
    public class OtherInputOptions : OptionsSubsection
    {
        protected override string Header => "Other";

        [Initializer]
        private void Load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "OS TabletPC support",
                    Bindable = config.GetBindable<bool>(OsuConfig.Tablet)
                },
                new CheckBoxOption
                {
                    LabelText = "Wiimote/TaTaCon Drum Support",
                    Bindable = config.GetBindable<bool>(OsuConfig.Wiimote)
                },
            };
        }
    }
}

