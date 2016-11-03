using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class LanguageOptions : OptionsSubsection
    {
        public LanguageOptions()
        {
            Header = "Language";
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                new BasicCheckBox
                {
                    Children = new[] { new SpriteText { Text = "Prefer metadata in original language" } }
                },
                new BasicCheckBox
                {
                    Children = new[] { new SpriteText { Text = "Use alternative font for chat display" } }
                },
            };
        }
    }
}