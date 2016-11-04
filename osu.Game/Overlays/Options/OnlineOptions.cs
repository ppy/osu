using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class OnlineOptions : OptionsSection
    {
        protected override string Header => "Online";
    
        public OnlineOptions()
        {
            Children = new Drawable[]
            {
                new AlertsPrivacyOptions(),
                new OnlineIntegrationOptions(),
                new InGameChatOptions(),
            };
        }
    }
}