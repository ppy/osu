// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public class DownloadNotification : ProgressNotification
    {
        public override bool IsImportant => false;

        protected override Notification CreateCompletionNotification() => new SilencedProgressCompletionNotification
        {
            Activated = CompletionClickAction,
            Text = CompletionText
        };

        private class SilencedProgressCompletionNotification : ProgressCompletionNotification
        {
            public override bool IsImportant => false;
        }
    }
}
