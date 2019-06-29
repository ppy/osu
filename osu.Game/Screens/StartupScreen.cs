// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays;

namespace osu.Game.Screens
{
    /// <summary>
    /// A screen which is shown once as part of the startup procedure.
    /// </summary>
    public abstract class StartupScreen : OsuScreen
    {
        public override bool AllowBackButton => false;

        public override bool HideOverlaysOnEnter => true;

        public override bool CursorVisible => false;

        public override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;
    }
}
