// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Extensions;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Verify.Components
{
    public class Issue
    {
        /// <summary>
        /// The time which this issue is associated with, if any, otherwise null.
        /// </summary>
        public double? Time;

        /// <summary>
        /// The hitobjects which this issue is associated with. Empty by default.
        /// </summary>
        public IReadOnlyList<HitObject> HitObjects;

        /// <summary>
        /// The template which this issue is using. This provides properties
        /// such as the <see cref="IssueTemplate.IssueType"/>, and the
        /// <see cref="IssueTemplate.UnformattedMessage"/>.
        /// </summary>
        public IssueTemplate Template;

        /// <summary>
        /// The arguments that give this issue its context, based on the
        /// <see cref="IssueTemplate"/>. These are then substituted into the
        /// <see cref="IssueTemplate.UnformattedMessage"/>.
        /// E.g. timestamps, which diff is being compared to, what some volume is, etc.
        /// </summary>
        public object[] Arguments;

        public Issue(IssueTemplate template, params object[] args)
        {
            Time = null;
            HitObjects = System.Array.Empty<HitObject>();
            Template = template;
            Arguments = args;

            if (template.Origin == null)
            {
                throw new ArgumentException(
                    "A template had no origin. Make sure the `Templates()` method contains all templates used."
                );
            }
        }

        public Issue(double? time, IssueTemplate template, params object[] args)
            : this(template, args)
        {
            Time = time;
        }

        public Issue(IEnumerable<HitObject> hitObjects, IssueTemplate template, params object[] args)
            : this(template, args)
        {
            Time = hitObjects.FirstOrDefault()?.StartTime;
            HitObjects = hitObjects.ToArray();
        }

        public override string ToString()
        {
            return Template.Message(Arguments);
        }

        public string GetEditorTimestamp()
        {
            // TODO: Editor timestamp formatting is handled in https://github.com/ppy/osu/pull/12030
            // We may be able to use that here too (if we decouple it from the HitObjectComposer class).

            if (Time == null)
                return string.Empty;

            return Time.Value.ToEditorFormattedString();
        }
    }
}
