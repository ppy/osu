// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneBarLine : ManiaSkinnableTestScene
    {
        [Test]
        public void TestMinor()
        {
            AddStep("Create barlines", () => recreate());
        }

        private void recreate(Func<IEnumerable<BarLine>>? createBarLines = null)
        {
            var stageDefinitions = new List<StageDefinition>
            {
                new StageDefinition(4),
            };

            SetContents(_ => new ManiaPlayfield(stageDefinitions).With(s =>
            {
                if (createBarLines != null)
                {
                    var barLines = createBarLines();

                    foreach (var b in barLines)
                        s.Add(b);

                    return;
                }

                for (int i = 0; i < 64; i++)
                {
                    s.Add(new BarLine
                    {
                        StartTime = Time.Current + i * 500,
                        Major = i % 4 == 0,
                    });
                }
            }));
        }
    }
}
