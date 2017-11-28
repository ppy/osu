// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;

namespace osu.Game.Notifications
{
    public interface IHasFollowUpNotifications
    {
        List<Notification> FollowUpNotifications { get; }
        event Action ProgressCompleted;
    }
}
