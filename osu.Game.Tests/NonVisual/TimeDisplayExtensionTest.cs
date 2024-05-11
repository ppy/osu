// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Extensions;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class TimeDisplayExtensionTest
    {
        private static readonly object[][] editor_formatted_duration_tests =
        {
            new object[] { new TimeSpan(0, 0, 0, 0, 50), "00:00:050" },
            new object[] { new TimeSpan(0, 0, 0, 10, 50), "00:10:050" },
            new object[] { new TimeSpan(0, 0, 5, 10), "05:10:000" },
            new object[] { new TimeSpan(0, 1, 5, 10), "65:10:000" },
        };

        [TestCaseSource(nameof(editor_formatted_duration_tests))]
        public void TestEditorFormat(TimeSpan input, string expectedOutput)
        {
            Assert.AreEqual(expectedOutput, input.ToEditorFormattedString());
        }

        private static readonly object[][] formatted_duration_tests =
        {
            new object[] { new TimeSpan(0, 0, 10), "00:10" },
            new object[] { new TimeSpan(0, 5, 10), "05:10" },
            new object[] { new TimeSpan(1, 5, 10), "01:05:10" },
            new object[] { new TimeSpan(1, 1, 5, 10), "01:01:05:10" },
        };

        [TestCaseSource(nameof(formatted_duration_tests))]
        public void TestFormattedDuration(TimeSpan input, string expectedOutput)
        {
            Assert.AreEqual(expectedOutput, input.ToFormattedDuration().ToString());
        }
    }
}
