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
        /// The check that this template originates from.
        /// </summary>
        public readonly ICheck Check;

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

        public IssueTemplate(ICheck check, IssueType type, string unformattedMessage)
        {
            Check = check;
            Type = type;
            UnformattedMessage = unformattedMessage;
        }

        /// <summary>
        /// Returns the formatted message given the arguments used to format it.
        /// </summary>
        /// <param name="args">The arguments used to format the message.</param>
        public string GetMessage(params object[] args) => UnformattedMessage.FormatWith(args);

        /// <summary>
        /// Returns the colour corresponding to the type of this issue.
        /// </summary>
        public Colour4 Colour
        {
            get
            {
                switch (Type)
                {
                    case IssueType.Problem:
                        return problem_red;

                    case IssueType.Warning:
                        return warning_yellow;

                    case IssueType.Negligible:
                        return negligible_green;

                    case IssueType.Error:
                        return error_gray;

                    default:
                        return Color4.White;
                }
            }
        }
    }
}
