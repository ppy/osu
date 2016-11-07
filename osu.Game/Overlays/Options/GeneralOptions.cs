using System;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Options
{
    public class GeneralOptions : OptionsSection
    {
        protected override string Header => "General";
        public override FontAwesome Icon => FontAwesome.fa_gear;
    
        public GeneralOptions()
        {
            Children = new Drawable[]
            {
                new LoginOptions(),
                new LanguageOptions(),
                new UpdateOptions(),
            };
        }
    }
}

