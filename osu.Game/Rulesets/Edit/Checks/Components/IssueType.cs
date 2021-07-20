// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    /// <summary>
    /// The type, or severity, of an issue.
    /// </summary>
    public enum IssueType
    {
        /// <summary> A must-fix in the vast majority of cases. </summary>
        Problem,

        /// <summary> A possible mistake. Often requires critical thinking.  </summary>
        Warning,

        // TODO: Try/catch all checks run and return error templates if exceptions occur.
        /// <summary> An error occurred and a complete check could not be made. </summary>
        Error,

        /// <summary> A possible mistake so minor/unlikely that it can often be safely ignored. </summary>
        Negligible,
    }
}
