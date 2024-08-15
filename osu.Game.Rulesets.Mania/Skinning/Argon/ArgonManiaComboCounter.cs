// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public partial class ArgonManiaComboCounter : ArgonComboCounter
    {
        protected override bool DisplayXSymbol => false;

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; } = null!;

        private IBindable<ScrollingDirection> direction = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // the logic of flipping the position of the combo counter w.r.t. the direction does not work with "Closest" anchor,
            // because it always forces the anchor to be top or bottom based on scrolling direction.
            UsesFixedAnchor = true;

            direction = scrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => updateAnchor());

            // two schedules are required so that updateAnchor is executed in the next frame,
            // which is when the combo counter receives its Y position by the default layout in ArgonManiaSkinTransformer.
            Schedule(() => Schedule(updateAnchor));
        }

        private void updateAnchor()
        {
            // if the anchor isn't a vertical center, set top or bottom anchor based on scroll direction
            if (!Anchor.HasFlag(Anchor.y1))
            {
                Anchor &= ~(Anchor.y0 | Anchor.y2);
                Anchor |= direction.Value == ScrollingDirection.Up ? Anchor.y2 : Anchor.y0;
            }

            // since we flip the vertical anchor when changing scroll direction,
            // we can use the sign of the Y value as an indicator to make the combo counter displayed correctly.
            if ((Y < 0 && direction.Value == ScrollingDirection.Down) || (Y > 0 && direction.Value == ScrollingDirection.Up))
                Y = -Y;
        }
    }
}
