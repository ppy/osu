using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Tau.Objects;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Tau.Edit.Blueprints
{
    public class HardBeatPlacementBlueprint : PlacementBlueprint
    {
        public HardBeatPlacementBlueprint()
            : base(new HardBeat())
        {
            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Yellow.Opacity(.2f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 5,
                    BorderColour = Color4.Yellow,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        },
                    }
                },
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                EndPlacement(true);

                return true;
            }

            return base.OnMouseDown(e);
        }
    }
}
