// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Options.Sections.Online;

namespace osu.Game.Overlays.Options.Sections
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
                new IntegrationOptions(),
            };
        }
    }
}