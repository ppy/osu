// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Input
{
    public class ActionMappingInputManager<T> : PassThroughInputManager
        where T : struct
    {
        protected IDictionary<Key, T> Mappings { get; set; }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            mapKey(state, args.Key);
            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            mapKey(state, args.Key);
            return base.OnKeyUp(state, args);
        }

        private void mapKey(InputState state, Key key)
        {
            T mappedData;
            if (Mappings.TryGetValue(key, out mappedData))
                state.Data = mappedData;
        }
    }
}
