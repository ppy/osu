// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing.Input;

namespace osu.Game.Tests.Visual
{
    public abstract class ManualInputManagerTestCase : OsuTestCase
    {
        protected override Container<Drawable> Content => InputManager;
        protected readonly ManualInputManager InputManager;

        protected ManualInputManagerTestCase()
        {
            base.Content.Add(InputManager = new ManualInputManager { UseParentInput = true });
        }

        /// <summary>
        /// Returns input back to the user.
        /// </summary>
        protected void ReturnUserInput()
        {
            AddStep("Return user input", () => InputManager.UseParentInput = true);
        }
    }
}
