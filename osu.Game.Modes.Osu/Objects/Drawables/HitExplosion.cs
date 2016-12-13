//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class HitExplosion : FlowContainer
    {
        private readonly OsuJudgementInfo judgement;
        private SpriteText line1;
        private SpriteText line2;

        public HitExplosion(OsuJudgementInfo judgement, OsuHitObject h = null)
        {
            this.judgement = judgement;
            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;

            Direction = FlowDirection.VerticalOnly;
            Spacing = new Vector2(0, 2);
            Position = (h?.EndPosition ?? Vector2.Zero) + judgement.PositionOffset;

            Children = new Drawable[]
            {
                line1 = new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = judgement.Score.GetDescription(),
                    Font = @"Venera",
                    TextSize = 16,
                },
                line2 = new SpriteText
                {
                    Text = judgement.Combo.GetDescription(),
                    Font = @"Venera",
                    TextSize = 11,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (judgement.Result == HitResult.Miss)
            {
                FadeInFromZero(60);

                ScaleTo(1.6f);
                ScaleTo(1, 100, EasingTypes.In);

                MoveToOffset(new Vector2(0, 100), 800, EasingTypes.InQuint);
                RotateTo(40, 800, EasingTypes.InQuint);

                Delay(600);
                FadeOut(200);
            }
            else
            {
                line1.TransformSpacingTo(new Vector2(14, 0), 1800, EasingTypes.OutQuint);
                line2.TransformSpacingTo(new Vector2(14, 0), 1800, EasingTypes.OutQuint);
                FadeOut(500);
            }

            switch (judgement.Result)
            {
                case HitResult.Miss:
                    Colour = Color4.Red;
                    break;
            }

            Expire();
        }
    }
}