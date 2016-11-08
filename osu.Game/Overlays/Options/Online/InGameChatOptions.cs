using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Online
{
    public class InGameChatOptions : OptionsSubsection
    {
        protected override string Header => "In-game Chat";
    
        public InGameChatOptions()
        {
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "Filter offensive words" },
                new BasicCheckBox { LabelText = "Filter foreign characters" },
                new BasicCheckBox { LabelText = "Log private messages" },
                new BasicCheckBox { LabelText = "Block private messages from non-friends" },
                new SpriteText { Text = "Chat ignore list (space-seperated list)" },
                new TextBox { Height = 20, RelativeSizeAxes = Axes.X },
                new SpriteText { Text = "Chat highlight words (space-seperated list)" },
                new TextBox { Height = 20, RelativeSizeAxes = Axes.X },
            };
        }
    }
}