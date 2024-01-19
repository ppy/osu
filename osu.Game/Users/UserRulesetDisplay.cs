// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Users
{
    public partial class UserRulesetDisplay : FillFlowContainer
    {
        private readonly APIUser user;

        public UserRulesetDisplay(APIUser user)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, RulesetStore rulesets)
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Vertical;

            Add(new OsuSpriteText
            {
                Font = OsuFont.GetFont(size: 12),
                Text = "Game Mode",
                Colour = colourProvider.Content1,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre
            });

            foreach (var r in rulesets.AvailableRulesets)
            {
                if (r.ShortName != (user?.PlayMode ?? @"osu"))
                    continue;

                Add(new UserRulesetIcon(r)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    // text in ProfileValueDisplay is shifted a little when drawn, let's account for that
                    Margin = new MarginPadding { Top = 2 },
                    Colour = colourProvider.Content2
                });
                break;
            }
        }

        private partial class UserRulesetIcon : CompositeDrawable, IHasTooltip
        {
            public LocalisableString TooltipText => ruleset.Name;

            private readonly RulesetInfo ruleset;

            public UserRulesetIcon(RulesetInfo ruleset)
            {
                this.ruleset = ruleset;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(30);
                InternalChild = new ConstrainedIconContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = ruleset.CreateInstance().CreateIcon(),
                    Size = new Vector2(20)
                };
            }
        }
    }
}
