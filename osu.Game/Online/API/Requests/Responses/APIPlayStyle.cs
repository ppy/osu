// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Online.API.Requests.Responses
{
    public enum APIPlayStyle
    {
        [Description("Keyboard")]
        Keyboard,

        [Description("Mouse")]
        Mouse,

        [Description("Tablet")]
        Tablet,

        [Description("Touch Screen")]
        Touch,
    }
}
