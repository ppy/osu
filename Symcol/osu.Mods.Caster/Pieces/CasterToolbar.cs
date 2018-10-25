using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace Symcol.osu.Mods.Caster.Pieces
{
    public class CasterToolbar : Container
    {
        public readonly Bindable<SelectedScreen> CurrentBibleScreen = new Bindable<SelectedScreen>();

        private readonly OsuTabControl<SelectedScreen> control;

        public CasterToolbar()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1 - 0.18f, 0.04f);

            AlwaysPresent = true;

            OsuColour color = new OsuColour();

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f
                },
                control = new OsuTabControl<SelectedScreen>
                {
                    Position = new Vector2(200, 0),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.4f,

                }
            };
            CurrentBibleScreen.BindTo(control.Current);
        }
    }
}
