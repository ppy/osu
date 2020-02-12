// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
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

        protected void Test(double expected, string name, params Mod[] mods)
            => Assert.AreEqual(expected, CreateDifficultyCalculator(getBeatmap(name)).Calculate(mods).StarRating);

        private WorkingBeatmap getBeatmap(string name)
        {
            using (var resStream = openResource($"{resource_namespace}.{name}.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);
                ((LegacyBeatmapDecoder)decoder).ApplyOffsets = false;

                var working = new TestWorkingBeatmap(decoder.Decode(stream));
                working.BeatmapInfo.Ruleset = CreateRuleset().RulesetInfo;

                return working;
            }
        }

        private Stream openResource(string name)
        {
            var localPath = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            return Assembly.LoadFrom(Path.Combine(localPath, $"{ResourceAssembly}.dll")).GetManifestResourceStream($@"{ResourceAssembly}.Resources.{name}");
        }

        protected abstract DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap);

        protected abstract Ruleset CreateRuleset();
    }
}
