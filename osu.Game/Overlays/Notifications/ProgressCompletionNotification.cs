// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Overlays.Notifications
{
    public class ProgressCompletionNotification : SimpleNotification
    {
        private ProgressNotification progressNotification;

        public ProgressCompletionNotification(ProgressNotification progressNotification)
            : base(@"Task has completed!")
        {
            this.progressNotification = progressNotification;
        }
    }
}