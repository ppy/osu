// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Options
{
    public class OptionsFooter : FillFlowContainer
    {
        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Top = 20, Bottom = 30 };

            var modes = new List<Drawable>();

            foreach (PlayMode m in Enum.GetValues(typeof(PlayMode)))
                modes.Add(new TextAwesome
                {
                    Icon = Ruleset.GetRuleset(m).Icon,
                    Colour = Color4.Gray,
                });

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