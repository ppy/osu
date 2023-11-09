// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Tests.Rulesets.Scoring
{
    [TestFixture]
    public class HitResultTest
    {
        [TestCase(new[] { HitResult.Perfect, HitResult.Great, HitResult.Good, HitResult.Ok, HitResult.Meh }, new[] { HitResult.Miss })]
        [TestCase(new[] { HitResult.LargeTickHit }, new[] { HitResult.LargeTickMiss })]
        [TestCase(new[] { HitResult.SmallTickHit }, new[] { HitResult.SmallTickMiss })]
        [TestCase(new[] { HitResult.LargeBonus, HitResult.SmallBonus }, new[] { HitResult.IgnoreMiss })]
        [TestCase(new[] { HitResult.IgnoreHit }, new[] { HitResult.IgnoreMiss, HitResult.ComboBreak })]
        public void TestValidResultPairs(HitResult[] maxResults, HitResult[] minResults)
        {
            HitResult[] unsupportedResults = HitResultExtensions.ALL_TYPES.Where(t => t != HitResult.IgnoreMiss && !minResults.Contains(t)).ToArray();

            Assert.Multiple(() =>
            {
                foreach (var max in maxResults)
                {
                    foreach (var min in minResults)
                        Assert.DoesNotThrow(() => HitResultExtensions.ValidateHitResultPair(max, min), $"{max} + {min} should be supported.");

                    foreach (var unsupported in unsupportedResults)
                        Assert.Throws<ArgumentOutOfRangeException>(() => HitResultExtensions.ValidateHitResultPair(max, unsupported), $"{max} + {unsupported} should not be supported.");
                }
            });
        }
    }
}
