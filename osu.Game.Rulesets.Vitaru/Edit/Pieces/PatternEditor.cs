using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using Symcol.Core.Graphics.UserInterface;

namespace osu.Game.Rulesets.Vitaru.Edit.Pieces
{
    public class PatternEditor : SymcolWindow
    {
        public PatternEditor() : base(new Vector2(300, 400))
        {
            Scale = new Vector2(2);
            WindowTitle.Text = "Pattern Editor";
            WindowContent.Child = new Box
            {
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Both,
                Alpha = 0.25f
            };
        }
    }
}
