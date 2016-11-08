using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Online
{
    public class PrivacyOptions : OptionsSubsection
    {
        protected override string Header => "Privacy";
    
        public PrivacyOptions()
        {
            // TODO: this should probably be split into Alerts and Privacy
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "Share your city location with others" },
                new BasicCheckBox { LabelText = "Allow multiplayer game invites from all users" },
                new BasicCheckBox { LabelText = "Block private messages from non-friends" },
            };
        }
    }
}