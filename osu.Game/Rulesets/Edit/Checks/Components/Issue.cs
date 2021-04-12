// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Extensions;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit.Checks.Components
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
        /// The template which this issue is using. This provides properties such as the <see cref="IssueType"/>, and the <see cref="IssueTemplate.UnformattedMessage"/>.
        /// </summary>
        public IssueTemplate Template;

        /// <summary>
        /// The check that this issue originates from.
        /// </summary>
        public ICheck Check;

        /// <summary>
        /// The arguments that give this issue its context, based on the <see cref="IssueTemplate"/>. These are then substituted into the <see cref="IssueTemplate.UnformattedMessage"/>.
        /// This could for instance include timestamps, which diff is being compared to, what some volume is, etc.
        /// </summary>
        public object[] Arguments;

        public Issue(ICheck check, IssueTemplate template, params object[] args)
        {
            Check = check;
            Time = null;
            HitObjects = Array.Empty<HitObject>();
            Template = template;
            Arguments = args;
        }

        public Issue(ICheck check, double? time, IssueTemplate template, params object[] args)
            : this(check, template, args)
        {
            Time = time;
        }

        public Issue(ICheck check, HitObject hitObject, IssueTemplate template, params object[] args)
            : this(check, template, args)
        {
            Time = hitObject.StartTime;
            HitObjects = new[] { hitObject };
        }

        public Issue(ICheck check, IEnumerable<HitObject> hitObjects, IssueTemplate template, params object[] args)
            : this(check, template, args)
        {
            var hitObjectList = hitObjects.ToList();

            Time = hitObjectList.FirstOrDefault()?.StartTime;
            HitObjects = hitObjectList;
        }

        public override string ToString() => Template.GetMessage(Arguments);

        public string GetEditorTimestamp()
        {
            return Time == null ? string.Empty : Time.Value.ToEditorFormattedString();
        }
    }
}
