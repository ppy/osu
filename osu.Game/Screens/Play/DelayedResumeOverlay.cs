// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Framework.Threading;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Simple <see cref="ResumeOverlay"/> that resumes after 800ms.
    /// </summary>
    public partial class DelayedResumeOverlay : ResumeOverlay
    {
        protected override LocalisableString Message => string.Empty;

        private ScheduledDelegate? scheduledResume;

        protected override void PopIn()
        {
            base.PopIn();

            scheduledResume?.Cancel();
            scheduledResume = Scheduler.AddDelayed(Resume, 800);
        }

        protected override void PopOut()
        {
            base.PopOut();
            scheduledResume?.Cancel();
        }
    }
}
