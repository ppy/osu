// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public partial class TestSceneBarLine : ManiaSkinnableTestScene
    {
        [Test]
        public void TestMinor()
        {
            AddStep("Create barlines", recreate);
        }

        private void recreate()
        {
            var stageDefinitions = new List<StageDefinition>
            {
                new StageDefinition(4),
            };

            SetContents(_ =>
            {
                var maniaPlayfield = new ManiaPlayfield(stageDefinitions);

                // Must be scheduled so the pool is loaded before we try and retrieve from it.
                Schedule(() =>
                {
                    for (int i = 0; i < 64; i++)
                    {
                        maniaPlayfield.Add(new BarLine
                        {
                            StartTime = Time.Current + i * 500,
                            Major = i % 4 == 0,
                        });
                    }
                });

                return maniaPlayfield;
            });
        }
    }
}
