//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Online
{
    public class OnlineSection : OptionsSection
    {
        public override string Header => "Online";
        public override FontAwesome Icon => FontAwesome.fa_globe;

        public OnlineSection()
        {
            Children = new Drawable[]
            {
                new InGameChatOptions(),
                new PrivacyOptions(),
                new NotificationsOptions(),
                new OnlineIntegrationOptions(),
            };
        }
    }
}