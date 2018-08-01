// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseEditorSetupCircularButton : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    new SetupCircularButton { Text = "Default" },
                    new SetupCircularButton
                    {
                        Width = 200,
                        Text = "Wide",
                    },
                    new SetupCircularButton
                    {
                        Height = 100,
                        Text = "High"
                    },
                    new SetupCircularButton
                    {
                        Size = new Vector2(200, 100),
                        Text = "Wide 'n' High"
                    }
                }
            };
        }
    }
}
