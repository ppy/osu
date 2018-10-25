using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Core.Graphics.Containers;
using Symcol.osu.Core.Screens.Evast;
using Symcol.osu.Mods.Caster.CasterScreens;
using Symcol.osu.Mods.Caster.Pieces;

namespace Symcol.osu.Mods.Caster
{
    public class CasterScreen : BeatmapScreen
    {
        protected override bool HideOverlaysOnEnter => true;

        private readonly CasterToolbar toolbar;
        private readonly CasterControlPanel casterPanel;

        private readonly SymcolContainer screenSpace;

        public CasterScreen()
        {
            Children = new Drawable[]
            {
                toolbar = new CasterToolbar(),
                casterPanel = new CasterControlPanel(),
                new SymcolContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,

                    RelativeSizeAxes = Axes.Both,
                    Position = new Vector2(-16),
                    Size = new Vector2(0.8f, 0.9f),

                    Masking = true,
                    CornerRadius = 20,

                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.4f
                    }
                },
                screenSpace = new SymcolContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,

                    RelativeSizeAxes = Axes.Both,
                    Position = new Vector2(-16),
                    Size = new Vector2(0.8f, 0.9f),
                }
            };

            toolbar.CurrentBibleScreen.ValueChanged += value =>
            {
                switch (value)
                {
                    case SelectedScreen.Maps:
                        screenSpace.Child = new Maps(casterPanel);
                        break;
                    case SelectedScreen.Results:
                        screenSpace.Child = new Results(casterPanel);
                        break;
                    case SelectedScreen.Teams:
                        screenSpace.Child = new Teams(casterPanel);
                        break;
                }
            };
            toolbar.CurrentBibleScreen.TriggerChange();
        }
    }

    public enum SelectedScreen
    {
        Maps,
        Results,
        Teams,
    }
}
