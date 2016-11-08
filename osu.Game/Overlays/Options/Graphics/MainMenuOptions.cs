using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Graphics
{
    public class MainMenuOptions : OptionsSubsection
    {
        protected override string Header => "Main Menu";
    
        public MainMenuOptions()
        {
            Children = new[]
            {
                new BasicCheckBox { LabelText = "Snow" },
                new BasicCheckBox { LabelText = "Parallax" },
                new BasicCheckBox { LabelText = "Menu tips" },
                new BasicCheckBox { LabelText = "Interface voices" },
                new BasicCheckBox { LabelText = "osu! music theme" },
            };
        }
    }
}