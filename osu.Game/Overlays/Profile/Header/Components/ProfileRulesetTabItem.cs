// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ProfileRulesetTabItem : OverlayRulesetTabItem
    {
        private readonly SpriteIcon icon;

        public new Color4 AccentColour
        {
            get => base.AccentColour;
            set => base.AccentColour = icon.Colour = value;
        }

        private bool isDefault;

        public bool IsDefault
        {
            get => isDefault;
            set
            {
                if (isDefault == value)
                    return;

                isDefault = value;

                icon.FadeTo(isDefault ? 1 : 0, 200, Easing.OutQuint);
            }
        }

        public ProfileRulesetTabItem(RulesetInfo value)
            : base(value)
        {
            Add(icon = new SpriteIcon
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Alpha = 0,
                AlwaysPresent = true,
                Icon = FontAwesome.Solid.Star,
                Size = new Vector2(12),
            });
        }

        protected override void UpdateState()
        {
            base.UpdateState();
            icon.FadeColour(GetColour(), 120, Easing.OutQuint);
        }
    }
}
