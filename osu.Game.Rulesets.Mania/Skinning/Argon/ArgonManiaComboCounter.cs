// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public partial class ArgonManiaComboCounter : ComboCounter, ISerialisableDrawable
    {
        private OsuSpriteText text = null!;

        protected override double RollingDuration => 500;
        protected override Easing RollingEasing => Easing.OutQuint;

        bool ISerialisableDrawable.SupportsClosestAnchor => false;

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            Current.BindTo(scoreProcessor.Combo);
            Current.BindValueChanged(combo =>
            {
                if (combo.OldValue == 0 && combo.NewValue > 0)
                    text.FadeIn(200, Easing.OutQuint);
                else if (combo.OldValue > 0 && combo.NewValue == 0)
                {
                    if (combo.OldValue > 1)
                        text.FlashColour(Color4.Red, 2000, Easing.OutQuint);

                    text.FadeOut(200, Easing.InQuint);
                }
            });

            UsesFixedAnchor = true;
        }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; } = null!;

        private IBindable<ScrollingDirection> direction = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            text.Alpha = Current.Value > 0 ? 1 : 0;

            direction = scrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => updateAnchor());

            // two schedules are required so that updateAnchor is executed in the next frame,
            // which is when the combo counter receives its Y position by the default layout in ArgonManiaSkinTransformer.
            Schedule(() => Schedule(updateAnchor));
        }

        private void updateAnchor()
        {
            Anchor &= ~(Anchor.y0 | Anchor.y2);
            Anchor |= direction.Value == ScrollingDirection.Up ? Anchor.y2 : Anchor.y0;

            // since we flip the vertical anchor when changing scroll direction,
            // we can use the sign of the Y value as an indicator to make the combo counter displayed correctly.
            if ((Y < 0 && direction.Value == ScrollingDirection.Down) || (Y > 0 && direction.Value == ScrollingDirection.Up))
                Y = -Y;
        }

        protected override IHasText CreateText() => text = new OsuSpriteText
        {
            Font = OsuFont.Torus.With(size: 32, fixedWidth: true),
        };
    }
}
