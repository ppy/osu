// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Utils
{
    /// <summary>
    /// Log to sentry without showing an error notification to the user.
    /// </summary>
    /// <remarks>
    /// This can be used to convey important diagnostics to us developers without
    /// getting in the user's way. Should be used sparingly.</remarks>
    internal class SentryOnlyDiagnosticsException : Exception
    {
        public SentryOnlyDiagnosticsException(string message)
            : base(message)
        {
        }
    }
}
