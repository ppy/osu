using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class OnlineOptions : OptionsSection
    {
        public OnlineOptions()
        {
            Header = "Online";
            Children = new Drawable[]
            {
                new AlertsPrivacyOptions(),
                new OnlineIntegrationOptions(),
                new InGameChatOptions(),
            };
        }
    }
}