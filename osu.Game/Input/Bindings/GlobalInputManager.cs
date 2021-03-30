// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;

namespace osu.Game.Input.Bindings
{
    public class GlobalInputManager : PassThroughInputManager
    {
        public readonly GlobalActionContainer GlobalBindings;

        protected override Container<Drawable> Content { get; }

        public GlobalInputManager(OsuGameBase game)
        {
            InternalChildren = new Drawable[]
            {
                Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // to avoid positional input being blocked by children, ensure the GlobalActionContainer is above everything.
                GlobalBindings = new GlobalActionContainer(game, true)
                {
                    GetInputQueue = () => NonPositionalInputQueue.ToArray()
                },
            };
        }
    }
}
