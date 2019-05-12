// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;
using DebugUtils = osu.Game.Utils.DebugUtils;

namespace osu.Game.Overlays.Settings
{
    public class SettingsFooter : FillFlowContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours, RulesetStore rulesets)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Top = 20, Bottom = 30 };

            var modes = new List<Drawable>();

            foreach (var ruleset in rulesets.AvailableRulesets)
            {
                var icon = new ConstrainedIconContainer
                {
                    Icon = ruleset.CreateInstance().CreateIcon(),
                    Colour = Color4.Gray,
                    Size = new Vector2(20),
                };

                modes.Add(icon);
            }

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Direction = FillDirection.Full,
                    AutoSizeAxes = Axes.Both,
                    Children = modes,
                    Spacing = new Vector2(5),
                    Padding = new MarginPadding { Bottom = 10 },
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = game.Name,
                    Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = OsuFont.GetFont(size: 14),
                    Text = game.Version,
                    Colour = DebugUtils.IsDebug ? colours.Red : Color4.White,
                },
            };
        }
    }
}
