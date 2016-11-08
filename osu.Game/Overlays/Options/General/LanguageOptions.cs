using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.General
{
    public class LanguageOptions : OptionsSubsection
    {
        protected override string Header => "Language";
        private CheckBoxOption showUnicode;
    
        public LanguageOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                showUnicode = new CheckBoxOption { LabelText = "Prefer metadata in original language" },
                new BasicCheckBox { LabelText = "Use alternative font for chat display" },
            };
        }
        
        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                showUnicode.Bindable = osuGame.Config.GetBindable<bool>(Configuration.OsuConfig.ShowUnicode);
            }
        }
    }
}
