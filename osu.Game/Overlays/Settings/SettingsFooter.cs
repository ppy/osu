// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings
{
    public class SettingsFooter : FillFlowContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, RulesetStore rulesets)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Top = 20, Bottom = 30, Horizontal = SettingsPanel.CONTENT_MARGINS };

            var modes = new List<Drawable>();

            foreach (var ruleset in rulesets.AvailableRulesets)
            {
                var icon = new ConstrainedIconContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
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
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
                new BuildDisplay(game.Version, DebugUtils.IsDebugBuild)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }
            };
        }

        private class BuildDisplay : OsuAnimatedButton
        {
            private readonly string version;
            private readonly bool isDebug;

            [Resolved]
            private OsuColour colours { get; set; }

            public BuildDisplay(string version, bool isDebug)
            {
                this.version = version;
                this.isDebug = isDebug;

                Content.RelativeSizeAxes = Axes.Y;
                Content.AutoSizeAxes = AutoSizeAxes = Axes.X;
                Height = 20;
            }

            [BackgroundDependencyLoader(true)]
            private void load(ChangelogOverlay changelog)
            {
                if (!isDebug)
                    Action = () => changelog?.ShowBuild(OsuGameBase.CLIENT_STREAM_NAME, version);

                Add(new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 16),

                    Text = version,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(5),
                    Colour = isDebug ? colours.Red : Color4.White,
                });
            }
        }
    }
}
