// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Overlays.Notifications
{
    public interface IHasCompletionTarget
    {
        Action<Notification> CompletionTarget { get; set; }
    }
}
