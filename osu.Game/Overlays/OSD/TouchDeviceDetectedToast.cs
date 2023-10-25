// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.OSD
{
    public partial class TouchDeviceDetectedToast : Toast
    {
        public TouchDeviceDetectedToast()
            : base("osu!", "Touch device detected", "Touch Device mod applied to score")
        {
        }
    }
}
