using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class InGameChatOptions : OptionsSubsection
    {
        protected override string Header => "In-game Chat";

        private CheckBoxOption filterWords, filterForeign, logPMs, blockPMs;
        private TextBoxOption chatIgnoreList, chatHighlightWords;

        public InGameChatOptions()
        {
            Children = new Drawable[]
            {
                filterWords = new CheckBoxOption { LabelText = "Filter offensive words" },
                filterForeign = new CheckBoxOption { LabelText = "Filter foreign characters" },
                logPMs = new CheckBoxOption { LabelText = "Log private messages" },
                blockPMs = new CheckBoxOption { LabelText = "Block private messages from non-friends" },
                new SpriteText { Text = "Chat ignore list (space-seperated list)" },
                chatIgnoreList = new TextBoxOption { Height = 20, RelativeSizeAxes = Axes.X },
                new SpriteText { Text = "Chat highlight words (space-seperated list)" },
                chatHighlightWords = new TextBoxOption { Height = 20, RelativeSizeAxes = Axes.X },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                filterWords.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ChatFilter);
                filterForeign.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ChatRemoveForeign);
                logPMs.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.LogPrivateMessages);
                blockPMs.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.BlockNonFriendPM);
                chatIgnoreList.Bindable = osuGame.Config.GetBindable<string>(OsuConfig.IgnoreList);
                chatHighlightWords.Bindable = osuGame.Config.GetBindable<string>(OsuConfig.HighlightWords);
            }
        }
    }
}