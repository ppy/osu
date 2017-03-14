// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarUserButton : ToolbarButton, IOnlineComponent
    {
        private Avatar avatar;

        public ToolbarUserButton()
        {
            AutoSizeAxes = Axes.X;

            DrawableText.Font = @"Exo2.0-MediumItalic";

            Add(new OpaqueBackground { Depth = 1 });

            Flow.Add(avatar = new Avatar());
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            api.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                default:
                    Text = @"Guest";
                    avatar.UserId = 1;
                    break;
                case APIState.Online:
                    Text = api.Username;
                    avatar.UserId = api.LocalUser.Value.Id;
                    break;
            }
        }
    }
}
