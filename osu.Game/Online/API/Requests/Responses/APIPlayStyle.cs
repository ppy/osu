// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Online.API.Requests.Responses
{
    public enum APIPlayStyle
    {
        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.DeviceKeyboard))]
        Keyboard,

        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.DeviceMouse))]
        Mouse,

        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.DeviceTablet))]
        Tablet,

        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.DeviceTouch))]
        Touch,
    }
}
