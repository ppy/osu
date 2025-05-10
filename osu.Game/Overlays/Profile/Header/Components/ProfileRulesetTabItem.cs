// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class ProfileRulesetTabItem : OverlayRulesetTabItem
    {
        private bool isDefault;

        public bool IsDefault
        {
            get => isDefault;
            set
            {
                if (isDefault == value)
                    return;

                isDefault = value;

                icon.Alpha = isDefault ? 1 : 0;
            }
        }

        protected override Color4 AccentColour
        {
            get => base.AccentColour;
            set
            {
                base.AccentColour = value;
                icon.FadeColour(value, 120, Easing.OutQuint);
            }
        }

        private readonly SpriteIcon icon;

        public ProfileRulesetTabItem(RulesetInfo value)
            : base(value)
        {
            Add(icon = new DefaultRulesetIcon { Alpha = 0 });
        }

        public partial class DefaultRulesetIcon : SpriteIcon, IHasTooltip
        {
            public LocalisableString TooltipText => UsersStrings.ShowEditDefaultPlaymodeIsDefaultTooltip;

            public DefaultRulesetIcon()
            {
                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;
                Icon = FontAwesome.Solid.Star;
                Size = new Vector2(12);
            }
        }
    }
}
