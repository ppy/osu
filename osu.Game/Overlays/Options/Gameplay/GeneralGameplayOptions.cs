using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class GeneralGameplayOptions : OptionsSubsection
    {
        protected override string Header => "General";
        
        public GeneralGameplayOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Background dim: TODO slider" },
                new SpriteText { Text = "Progress display: TODO dropdown" },
                new SpriteText { Text = "Score meter type: TODO dropdown" },
                new SpriteText { Text = "Score meter size: TODO slider" },
                new BasicCheckBox { LabelText = "Always show key overlay" },
                new BasicCheckBox { LabelText = "Show approach circle on first \"Hidden\" object" },
                new BasicCheckBox { LabelText = "Scale osu!mania scroll speed with BPM" },
                new BasicCheckBox { LabelText = "Remember osu!mania scroll speed per beatmap" },
            };
        }
    }
}