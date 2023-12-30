// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyManiaComboCounter : LegacyComboCounter, ISerialisableDrawable
    {
        private DrawableManiaRuleset maniaRuleset = null!;

        bool ISerialisableDrawable.SupportsClosestAnchor => false;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, DrawableRuleset ruleset)
        {
            maniaRuleset = (DrawableManiaRuleset)ruleset;

            DisplayedCountText.Anchor = Anchor.Centre;
            DisplayedCountText.Origin = Anchor.Centre;

            PopOutCountText.Anchor = Anchor.Centre;
            PopOutCountText.Origin = Anchor.Centre;
            PopOutCountText.Colour = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.ComboBreakColour)?.Value ?? Color4.Red;

            UsesFixedAnchor = true;
        }

        private IBindable<ScrollingDirection> direction = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            direction = maniaRuleset.ScrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => updateAnchor());

            // two schedules are required so that updateAnchor is executed in the next frame,
            // which is when the combo counter receives its Y position by the default layout in LegacyManiaSkinTransformer.
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

        protected override void OnCountIncrement()
        {
            base.OnCountIncrement();

            PopOutCountText.Hide();
            DisplayedCountText.ScaleTo(new Vector2(1f, 1.4f))
                              .ScaleTo(new Vector2(1f), 300, Easing.Out)
                              .FadeIn(120);
        }

        protected override void OnCountChange()
        {
            base.OnCountChange();

            PopOutCountText.Hide();
            DisplayedCountText.ScaleTo(1f);
        }

        protected override void OnCountRolling()
        {
            if (DisplayedCount > 0)
            {
                PopOutCountText.Text = FormatCount(DisplayedCount);
                PopOutCountText.FadeTo(0.8f).FadeOut(200)
                               .ScaleTo(1f).ScaleTo(4f, 200);

                DisplayedCountText.FadeTo(0.5f, 300);
            }

            base.OnCountRolling();
        }
    }
}
