// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseColumn : ManiaInputTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Column),
            typeof(ColumnBackground),
            typeof(ColumnKeyArea),
            typeof(ColumnHitObjectArea)
        };

        public TestCaseColumn()
            : base(2)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = new Vector2(20, 0),
                Children = new[]
                {
                    createColumn(ScrollingDirection.Up, ManiaAction.Key1),
                    createColumn(ScrollingDirection.Down, ManiaAction.Key2)
                }
            };
        }

        private Column createColumn(ScrollingDirection direction, ManiaAction action) => new Column(direction)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Height = 0.85f,
            AccentColour = Color4.OrangeRed,
            Action = action
        };
    }
}
