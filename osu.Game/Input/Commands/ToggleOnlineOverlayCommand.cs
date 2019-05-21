// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Commands;
using osu.Game.Online.API;

namespace osu.Game.Input.Commands
{
    public class ToggleOnlineOverlayCommand<T> : ICommand where T : OverlayContainer
    {
        private readonly OverlayContainer overlayContainer;

        public ToggleOnlineOverlayCommand(OverlayContainer overlay, APIAccess api)
        {
            overlayContainer = overlay;

            CanExecute = new Bindable<bool>(api.IsLoggedIn);
            api.LocalUser.BindValueChanged((_) => CanExecute.Value = api.IsLoggedIn);
        }

        public void Execute()
        {
            if (!CanExecute.Value && overlayContainer.State == Visibility.Hidden)
            {
                // TODO notify the user about required authentication
                return;
            }

            overlayContainer.ToggleVisibility();
        }

        public Bindable<bool> CanExecute { get; }
    }
}
