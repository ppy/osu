// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.ClicksPerSecond;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A target (generally always <see cref="DrawableRuleset"/>) which can attach various skinnable components.
    /// </summary>
    /// <remarks>
    /// Attach methods will give the target permission to prepare the component into a usable state, usually via
    /// calling methods on the component (attaching various gameplay devices).
    /// </remarks>
    public interface ICanAttachHUDPieces
    {
        void Attach(InputCountController inputCountController);
        void Attach(ClicksPerSecondController controller);
    }
}
