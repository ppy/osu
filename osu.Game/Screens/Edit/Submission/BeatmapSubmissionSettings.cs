// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;

namespace osu.Game.Screens.Edit.Submission
{
    public class BeatmapSubmissionSettings
    {
        public Bindable<BeatmapSubmissionTarget> Target { get; } = new Bindable<BeatmapSubmissionTarget>();

        public Bindable<bool> NotifyOnDiscussionReplies { get; } = new Bindable<bool>();
    }
}
