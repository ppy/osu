// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public class IssueTemplate
    {
        private static readonly Color4 problem_red = new Colour4(1.0f, 0.4f, 0.4f, 1.0f);
        private static readonly Color4 warning_yellow = new Colour4(1.0f, 0.8f, 0.2f, 1.0f);
        private static readonly Color4 negligible_green = new Colour4(0.33f, 0.8f, 0.5f, 1.0f);
        private static readonly Color4 error_gray = new Colour4(0.5f, 0.5f, 0.5f, 1.0f);

        /// <summary>
        /// The type, or severity, of an issue. This decides its priority.
        /// </summary>
        public enum IssueType
        {
            /// <summary> A must-fix in the vast majority of cases. </summary>
            Problem = 3,

            /// <summary> A possible mistake. Often requires critical thinking.  </summary>
            Warning = 2,

            // TODO: Try/catch all checks run and return error templates if exceptions occur.
            /// <summary> An error occurred and a complete check could not be made. </summary>
            Error = 1,

            // TODO: Negligible issues should be hidden by default.
            /// <summary> A possible mistake so minor/unlikely that it can often be safely ignored. </summary>
            Negligible = 0,
        }

        /// <summary>
        /// The check that this template originates from.
        /// </summary>
        public Check Origin;

        /// <summary>
        /// The type of the issue.
        /// </summary>
        public readonly IssueType Type;

        /// <summary>
        /// The unformatted message given when this issue is detected.
        /// This gets populated later when an issue is constructed with this template.
        /// E.g. "Inconsistent snapping (1/{0}) with [{1}] (1/{2})."
        /// </summary>
        public readonly string UnformattedMessage;

        public IssueTemplate(IssueType type, string unformattedMessage)
        {
            Type = type;
            UnformattedMessage = unformattedMessage;
        }

        /// <summary>
        /// Returns the formatted message given the arguments used to format it.
        /// </summary>
        /// <param name="args">The arguments used to format the message.</param>
        public string Message(params object[] args) => UnformattedMessage.FormatWith(args);

        /// <summary>
        /// Returns the colour corresponding to the type of this issue.
        /// </summary>
        public Colour4 TypeColour()
        {
            return Type switch
            {
                IssueType.Problem => problem_red,
                IssueType.Warning => warning_yellow,
                IssueType.Negligible => negligible_green,
                IssueType.Error => error_gray,
                _ => Color4.White
            };
        }
    }
}
