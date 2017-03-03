// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using osu.Game.Screens.Multiplayer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMultiCheckBox : TestCase
    {
        public override string Name => @"MultiCheckBox";
        public override string Description => @"Add some lobby filters";

        public override void Reset()
        {
            base.Reset();

            Add(new Container //Positionning container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.2f,0.1f),
                Children = new Drawable[]
                {
                    new MultiCheckBox
                    {
                        LabelText = "Owned beatmap",
                    },
                }
            });
        }
    }
}
