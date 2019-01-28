// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens;
using osu.Game.Overlays;

namespace osu.Game.Screens
{
    public interface IOsuScreen : IScreen
    {
        /// <summary>
        /// Whether the beatmap or ruleset should be allowed to be changed by the user or game.
        /// Used to mark exclusive areas where this is strongly prohibited, like gameplay.
        /// </summary>
        bool AllowBeatmapRulesetChange { get; }

        bool AllowExternalScreenChange { get; }

        /// <summary>
        /// Whether this <see cref="OsuScreen"/> allows the cursor to be displayed.
        /// </summary>
        bool CursorVisible { get; }

        /// <summary>
        /// Whether all overlays should be hidden when this screen is entered or resumed.
        /// </summary>
        bool HideOverlaysOnEnter { get; }

        /// <summary>
        /// Whether overlays should be able to be opened once this screen is entered or resumed.
        /// </summary>
        OverlayActivation InitialOverlayActivationMode { get; }
    }
}
