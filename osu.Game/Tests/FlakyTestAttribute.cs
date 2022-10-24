// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;

namespace osu.Game.Tests
{
    /// <summary>
    /// An attribute to mark any flaky tests.
    /// Will add a retry count unless environment variable `FAIL_FLAKY_TESTS` is set to `1`.
    /// </summary>
    public class FlakyTestAttribute : RetryAttribute
    {
        public FlakyTestAttribute()
            : this(10)
        {
        }

        public FlakyTestAttribute(int tryCount)
            : base(Environment.GetEnvironmentVariable("OSU_TESTS_FAIL_FLAKY") == "1" ? 1 : tryCount)
        {
        }
    }
}
