// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsFooter : FillFlowContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, RulesetStore rulesets)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Top = 20, Bottom = 30, Horizontal = SettingsPanel.CONTENT_MARGINS };

            FillFlowContainer modes;

            Children = new Drawable[]
            {
                modes = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Direction = FillDirection.Full,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
                new BuildDisplay(game.Version)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }
            };

            foreach (var ruleset in rulesets.AvailableRulesets)
            {
                try
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
                catch
                {
                    Logger.Log($"Could not create ruleset icon for {ruleset.Name}. Please check for an update from the developer.", level: LogLevel.Error);
                }
            }
        }

        private partial class BuildDisplay : OsuAnimatedButton
        {
            private readonly string version;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public BuildDisplay(string version)
            {
                this.version = version;

                Content.RelativeSizeAxes = Axes.Y;
                Content.AutoSizeAxes = AutoSizeAxes = Axes.X;
                Height = 20;
            }

            [BackgroundDependencyLoader]
            private void load(ChangelogOverlay? changelog)
            {
                Action = () => changelog?.ShowBuild(OsuGameBase.CLIENT_STREAM_NAME, version);

                Add(new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 16),

                    Text = version,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(5),
                    Colour = DebugUtils.IsDebugBuild ? colours.Red : Color4.White,
                });
            }
        }
    }
}
