using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class HitExplosion : FlowContainer
    {
        private SpriteText line1;
        private SpriteText line2;

        public HitExplosion(HitResult hitResult, ComboResult comboResult = ComboResult.None)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Direction = FlowDirection.VerticalOnly;
            Spacing = new Vector2(0, 2);

            Children = new Drawable[]
            {
                line1 = new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = hitResult.GetDescription(),
                    Font = @"Venera",
                    TextSize = 20,
                },
                line2 = new SpriteText
                {
                    Text = comboResult.GetDescription(),
                    Font = @"Venera",
                    TextSize = 14,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            line1.TransformSpacingTo(14, 1800, EasingTypes.OutQuint);
            line2.TransformSpacingTo(14, 1800, EasingTypes.OutQuint);
        }
    }
}