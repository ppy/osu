using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class HitExplosion : FlowContainer
    {
        private SpriteText line1;
        private SpriteText line2;

        public HitExplosion(OsuJudgementInfo judgement)
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
                    Text = judgement.Score.GetDescription(),
                    Font = @"Venera",
                    TextSize = 20,
                },
                line2 = new SpriteText
                {
                    Text = judgement.Combo.GetDescription(),
                    Font = @"Venera",
                    TextSize = 14,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            line1.TransformSpacingTo(new Vector2(14, 0), 1800, EasingTypes.OutQuint);
            line2.TransformSpacingTo(new Vector2(14, 0), 1800, EasingTypes.OutQuint);
        }
    }
}