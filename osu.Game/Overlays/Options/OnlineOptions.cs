using System;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class OnlineOptions : OptionsSection
    {
        protected override string Header => "Online";
        public override FontAwesome Icon => FontAwesome.fa_globe;
    
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