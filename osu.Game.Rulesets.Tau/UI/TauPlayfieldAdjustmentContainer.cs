using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using osuTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Tau.UI
{
    public class TauPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        private const float playfield_size_adjust = 3.5f / 3;

        public TauPlayfieldAdjustmentContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = new Vector2(playfield_size_adjust);

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillAspectRatio = 1,
                Child = content = new ScalingContainer { RelativeSizeAxes = Axes.Both }
            };
        }
    }
}
