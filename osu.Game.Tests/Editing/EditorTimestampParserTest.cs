// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class EditorTimestampParserTest
    {
        public static readonly object?[][] test_cases =
        {
            new object?[] { ":", false, null, null },
            new object?[] { "1", true, new TimeSpan(0, 0, 1, 0), null },
            new object?[] { "99", true, new TimeSpan(0, 0, 99, 0), null },
            new object?[] { "300", false, null, null },
            new object?[] { "1:2", true, new TimeSpan(0, 0, 1, 2), null },
            new object?[] { "1:02", true, new TimeSpan(0, 0, 1, 2), null },
            new object?[] { "1:92", false, null, null },
            new object?[] { "1:002", false, null, null },
            new object?[] { "1:02:3", true, new TimeSpan(0, 0, 1, 2, 3), null },
            new object?[] { "1:02:300", true, new TimeSpan(0, 0, 1, 2, 300), null },
            new object?[] { "1:02:3000", false, null, null },
            new object?[] { "1:02:300 ()", false, null, null },
            new object?[] { "1:02:300 (1,2,3)", true, new TimeSpan(0, 0, 1, 2, 300), "1,2,3" },
        };

        [TestCaseSource(nameof(test_cases))]
        public void TestTryParse(string timestamp, bool expectedSuccess, TimeSpan? expectedParsedTime, string? expectedSelection)
        {
            bool actualSuccess = EditorTimestampParser.TryParse(timestamp, out var actualParsedTime, out string? actualSelection);

            Assert.Multiple(() =>
            {
                Assert.That(actualSuccess, Is.EqualTo(expectedSuccess));
                Assert.That(actualParsedTime, Is.EqualTo(expectedParsedTime));
                Assert.That(actualSelection, Is.EqualTo(expectedSelection));
            });
        }
    }
}
