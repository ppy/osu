using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Online
{
    public class NotificationsOptions : OptionsSubsection
    {
        protected override string Header => "Notifications";
    
        public NotificationsOptions()
        {
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "Enable chat ticker" },
                new BasicCheckBox { LabelText = "Show a notification popup when someone says your name" },
                new BasicCheckBox { LabelText = "Show chat message notifications" },
                new BasicCheckBox { LabelText = "Play a sound when someone says your name" },
                new BasicCheckBox { LabelText = "Show notification popups instantly during gameplay" },
                new BasicCheckBox { LabelText = "Show notification popups when friends change status" },
            };
        }
    }
}