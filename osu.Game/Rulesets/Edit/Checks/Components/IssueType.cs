// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    /// <summary>
    /// The type, or severity, of an issue.
    /// </summary>
    public enum IssueType
    {
        /// <summary> A must-fix in the vast majority of cases. </summary>
        [Description("问题")]
        Problem,

        /// <summary> A possible mistake. Often requires critical thinking.  </summary>
        [Description("警告")]
        Warning,

        // TODO: Try/catch all checks run and return error templates if exceptions occur.
        /// <summary> An error occurred and a complete check could not be made. </summary>
        [Description("错误")]
        Error,

        // TODO: Negligible issues should be hidden by default.
        /// <summary> A possible mistake so minor/unlikely that it can often be safely ignored. </summary>
        [Description("小毛病")]
        Negligible,
    }
}
