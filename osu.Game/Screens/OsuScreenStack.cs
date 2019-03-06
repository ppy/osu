// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;

namespace osu.Game.Screens
{
    public class OsuScreenStack : ScreenStack
    {
        public OsuScreenStack()
        {
            ScreenExited += onExited;
        }

        public OsuScreenStack(IScreen baseScreen)
            : base(baseScreen)
        {
            ScreenExited += onExited;
        }

        private void onExited(IScreen prev, IScreen next) => (prev as OsuScreen)?.UnbindAllBindables();
    }
}
