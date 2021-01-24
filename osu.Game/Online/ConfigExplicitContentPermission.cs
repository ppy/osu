// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Online
{
    /// <summary>
    /// A type of <see cref="IExplicitContentPermission"/> that binds <see cref="UserAllowed"/> to <see cref="OsuSetting.ShowOnlineExplicitContent"/>'s value directly.
    /// </summary>
    internal class ConfigExplicitContentPermission : IExplicitContentPermission
    {
        public IBindable<bool> UserAllowed => userAllowed;

        private readonly Bindable<bool> userAllowed = new Bindable<bool>();

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable (hold reference)
        private readonly IBindable<bool> showExplicitContentSetting;

        public ConfigExplicitContentPermission(OsuConfigManager config)
        {
            showExplicitContentSetting = config.GetBindable<bool>(OsuSetting.ShowOnlineExplicitContent);
            showExplicitContentSetting.BindValueChanged(e => userAllowed.Value = e.NewValue, true);
        }
    }
}
