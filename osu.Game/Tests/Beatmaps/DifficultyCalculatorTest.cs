// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using System.Reflection;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public abstract class DifficultyCalculatorTest
    {
        private const string resource_namespace = "Testing.Beatmaps";

        protected abstract string ResourceAssembly { get; }

        protected void Test(double expectedStarRating, int expectedMaxCombo, string name, params Mod[] mods)
        {
            var attributes = CreateDifficultyCalculator(getBeatmap(name)).Calculate(mods);

            // Platform-dependent math functions (Pow, Cbrt, Exp, etc) may result in minute differences.
            Assert.That(attributes.StarRating, Is.EqualTo(expectedStarRating).Within(0.00001));
            Assert.That(attributes.MaxCombo, Is.EqualTo(expectedMaxCombo));
        }

        private IWorkingBeatmap getBeatmap(string name)
        {
            using (var resStream = openResource($"{resource_namespace}.{name}.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);

                ((LegacyBeatmapDecoder)decoder).ApplyOffsets = false;

                return new TestWorkingBeatmap(decoder.Decode(stream))
                {
                    BeatmapInfo =
                    {
                        Ruleset = CreateRuleset().RulesetInfo
                    }
                };
            }
        }

        private Stream openResource(string name)
        {
            string localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).AsNonNull();
            return Assembly.LoadFrom(Path.Combine(localPath, $"{ResourceAssembly}.dll")).GetManifestResourceStream($@"{ResourceAssembly}.Resources.{name}");
        }

        protected abstract DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap);

        protected abstract Ruleset CreateRuleset();
    }
}
