// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Settings
{
    public class SettingsFooter : FillFlowContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours, RulesetDatabase rulesets)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Top = 20, Bottom = 30 };

            var modes = new List<Drawable>();

            foreach (var ruleset in rulesets.AllRulesets)
            {
                modes.Add(new TextAwesome
                {
                    Icon = ruleset.CreateInstance().Icon,
                    Colour = Color4.Gray,
                    TextSize = 20
                });
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
                    TextSize = 18,
                    Font = @"Exo2.0-Bold",
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    TextSize = 14,
                    Text = game.Version,
                    Colour = game.IsDebug ? colours.Red : Color4.White,
                },
            };
        }
    }
}