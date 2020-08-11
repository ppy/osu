// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModSpunOut : OsuModTestScene
    {
        [Test]
        public void TestSpinnerAutoCompleted() => CreateModTest(new ModTestData
        {
            Mod = new OsuModSpunOut(),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Spinner
                    {
                        Position = new Vector2(256, 192),
                        StartTime = 500,
                        Duration = 2000
                    }
                }
            },
            PassCondition = () => Player.ChildrenOfType<DrawableSpinner>().Single().Progress >= 1
        });
    }
}
