// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    /// <summary>
    /// This signifies that a <see cref="Skin.GetDrawableComponent"/> call resolved a configuration created
    /// by a user in their skin. Generally this should be given priority over any local defaults or overrides.
    /// </summary>
    public partial class UserConfiguredLayoutContainer : Container
    {
    }
}
