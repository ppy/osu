using osu.Framework.Allocation;
using osu.Framework.Configuration;
using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Vitaru.Wiki.Sections
{
    public class CodeSection : WikiSection
    {
        public override string Title => "Code";

        private Bindable<Equations> selectedEquations = new Bindable<Equations> { Default = Equations.ConvertDifficultySettings };

        private WikiOptionEnumExplanation<Equations> equationsDescription;

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Add(new WikiParagraph("Don't worry, you don't have to speak C# to understand this section. " +
                        "This is just a place for people who want to know exactly whats going on under the hood without having to go digging through the code themselves. " +
                        "Its a place for the programmers to display things like the PP algorithm or exactly how certain spells calculate certain things to you, or your friends if you don't care.\n"));
            Content.Add(equationsDescription = new WikiOptionEnumExplanation<Equations>(selectedEquations));

            selectedEquations.ValueChanged += equations =>
            {
                switch (equations)
                {
                    case Equations.ConvertDifficultySettings:
                        equationsDescription.Description.Text = "Check back later!";
                        break;
                    case Equations.Difficulty:
                        equationsDescription.Description.Text = "I honestly have no idea how it works or if it actually does. It appears to work so I ain't gonna go back in there till people complain.";
                        break;
                    case Equations.PP:
                        equationsDescription.Description.Text = "Equation: pp = difficulty * Score.TotalScore * pp_multiplier;\n\n" +
                        "Where:\n" +
                        "difficulty = map star rating\n" +
                        "Score.TotalScore = score you got\n" +
                        "pp_multiplier = some number of my choosing. This will NEVER change EVER and be the same for EVERY play EVER!\n\n" +
                        "Personally I feel this is how all gamemodes should do it but ¯\\_(ツ)_/¯";
                        break;
                }
            };
            selectedEquations.TriggerChange();
        }
    }

    public enum Equations
    {
        [System.ComponentModel.Description("Difficulty Settings")]
        ConvertDifficultySettings,
        [System.ComponentModel.Description("Difficulty Calculation")]
        Difficulty,
        [System.ComponentModel.Description("PP Calulation")]
        PP
    }
}
