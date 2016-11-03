using System;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Options
{
    public class GeneralOptions : OptionsSection
    {
        public GeneralOptions(BasicStorage storage, APIAccess api)
        {
            Header = "General";
            Children = new Drawable[]
            {
                new LoginOptions(api),
                new LanguageOptions(),
                new UpdateOptions(storage),
            };
        }
    }
}

