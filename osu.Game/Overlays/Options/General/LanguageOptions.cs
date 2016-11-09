using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.General
{
    public class LanguageOptions : OptionsSubsection
    {
        protected override string Header => "Language";
        private CheckBoxOption showUnicode, altChatFont;

        public LanguageOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                showUnicode = new CheckBoxOption { LabelText = "Prefer metadata in original language" },
                altChatFont = new CheckBoxOption { LabelText = "Use alternative font for chat display" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                showUnicode.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ShowUnicode);
                altChatFont.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.AlternativeChatFont);
            }
        }
    }
}
