using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Graphics
{
    public class DetailOptions : OptionsSubsection
    {
        protected override string Header => "Detail Settings";
    
        public DetailOptions()
        {
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "Snaking sliders" },
                new BasicCheckBox { LabelText = "Background video" },
                new BasicCheckBox { LabelText = "Storyboards" },
                new BasicCheckBox { LabelText = "Combo bursts" },
                new BasicCheckBox { LabelText = "Hit lighting" },
                new BasicCheckBox { LabelText = "Shaders" },
                new BasicCheckBox { LabelText = "Softening filter" },
                new SpriteText { Text = "Screenshot format TODO: dropdown" }
            };
        }
    }
}