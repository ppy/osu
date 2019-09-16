// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Rankings;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneHumanizedNumber : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HumanizedNumber),
        };

        public TestSceneHumanizedNumber()
        {
            Add(new FillFlowContainer<HumanizedNumber>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    new HumanizedNumber(0)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new HumanizedNumber(999)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new HumanizedNumber(546895)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new HumanizedNumber(87546895)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new HumanizedNumber(456545546895)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new HumanizedNumber(76476545546895)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                }
            });
        }
    }
}
