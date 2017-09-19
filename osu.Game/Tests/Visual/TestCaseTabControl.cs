﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select.Filter;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseTabControl : OsuTestCase
    {
        public override string Description => @"Filter for song select";

        public TestCaseTabControl()
        {
            OsuSpriteText text;
            OsuTabControl<GroupMode> filter;
            Add(filter = new OsuTabControl<GroupMode>
            {
                Margin = new MarginPadding(4),
                Size = new Vector2(229, 24),
                AutoSort = true
            });
            Add(text = new OsuSpriteText
            {
                Text = "None",
                Margin = new MarginPadding(4),
                Position = new Vector2(275, 5)
            });

            filter.PinItem(GroupMode.All);
            filter.PinItem(GroupMode.RecentlyPlayed);

            filter.Current.ValueChanged += newFilter =>
            {
                text.Text = "Currently Selected: " + newFilter.ToString();
            };
        }
    }
}
