﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Online.Placeholders
{
    public class ClickablePlaceholder : Placeholder
    {
        public Action Action;

        public ClickablePlaceholder(string actionMessage, IconUsage icon)
        {
            OsuTextFlowContainer textFlow;

            AddArbitraryDrawable(new OsuAnimatedButton
            {
                AutoSizeAxes = Framework.Graphics.Axes.Both,
                Child = textFlow = new OsuTextFlowContainer(cp => cp.Font = cp.Font.With(size: TEXT_SIZE))
                {
                    AutoSizeAxes = Framework.Graphics.Axes.Both,
                    Margin = new Framework.Graphics.MarginPadding(5)
                },
                Action = () => Action?.Invoke()
            });

            textFlow.AddIcon(icon, i =>
            {
                i.Padding = new Framework.Graphics.MarginPadding { Right = 10 };
            });

            textFlow.AddText(actionMessage);
        }
    }
}
