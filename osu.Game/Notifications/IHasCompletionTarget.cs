// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Notifications
{
    public interface IHasCompletionTarget
    {
        Action<NotificationContainer> CompletionTarget { get; set; }
    }
}
