using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Online
{
    public class AlertsPrivacyOptions : OptionsSubsection
    {
        protected override string Header => "Alerts & Privacy";
    
        public AlertsPrivacyOptions()
        {
            // TODO: this should probably be split into Alerts and Privacy
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "Chat ticker" },
                new BasicCheckBox { LabelText = "Automatically hide chat during gameplay" },
                new BasicCheckBox { LabelText = "Show a notification popup when someone says your name" },
                new BasicCheckBox { LabelText = "Show chat message notifications" },
                new BasicCheckBox { LabelText = "Play a sound when someone says your name" },
                new BasicCheckBox { LabelText = "Share your city location with others" },
                new BasicCheckBox { LabelText = "Show spectators" },
                new BasicCheckBox { LabelText = "Automatically link beatmaps to spectators" },
                new BasicCheckBox { LabelText = "Show notification popups instantly during gameplay" },
                new BasicCheckBox { LabelText = "Show notification popups when friends change status" },
                new BasicCheckBox { LabelText = "Allow multiplayer game invites from all users" },
            };
        }
    }
}