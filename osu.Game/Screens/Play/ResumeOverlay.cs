// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;

namespace osu.Game.Screens.Play
{
    public abstract class ResumeOverlay : GameplayMenuOverlay
    {
        public Action ResumeAction { get; set; }
        public Action PauseAction { get; set; }

        protected override Action BackAction => PauseAction;

        public CursorContainer Cursor { get; set; }
        public PassThroughInputManager InputManager { get; set; }
    }
}
