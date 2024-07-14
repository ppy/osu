// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Graphics.UserInterface
{
    public partial class DangerousRoundedButton : RoundedButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.DangerousButtonColour;
        }
    }
}
