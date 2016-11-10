using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class PrivacyOptions : OptionsSubsection
    {
        protected override string Header => "Privacy";
    
        [Initializer]
        private void Load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Share your city location with others",
                    Bindable = config.GetBindable<bool>(OsuConfig.DisplayCityLocation)
                },
                new CheckBoxOption
                {
                    LabelText = "Allow multiplayer game invites from all users",
                    Bindable = config.GetBindable<bool>(OsuConfig.AllowPublicInvites)
                },
            };
        }
    }
}