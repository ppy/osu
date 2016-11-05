using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class LanguageOptions : OptionsSubsection
    {
        protected override string Header => "Language";
    
        public LanguageOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                new BasicCheckBox { LabelText = "Prefer metadata in original language" },
                new BasicCheckBox { LabelText = "Use alternative font for chat display" },
            };
        }
    }
}