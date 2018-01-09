// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface.Volume
{
    public class VolumeControlReceptor : Container, IKeyBindingHandler<GlobalAction>
    {
        public Func<GlobalAction, bool> ActionRequested;

        public bool OnPressed(GlobalAction action) => ActionRequested?.Invoke(action) ?? false;
        public bool OnReleased(GlobalAction action) => false;
    }
}
