// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Login;

namespace osu.Game.Overlays.AccountCreation
{
    public partial class ScreenEmailVerification : AccountCreationScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new SecondFactorAuthForm
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(20),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }
    }
}
