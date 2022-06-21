// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osuTK;

namespace osu.Game.Screens.Play
{
    public class SaveFailedScoreButton : CompositeDrawable
    {
        public Action? OnSave;

        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        private DownloadButton button;
        private ShakeContainer shakeContainer;

        public SaveFailedScoreButton()
        {
            InternalChild = shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new DownloadButton
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
            Size = new Vector2(50, 30);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            button.Action = () =>
            {
                switch (State.Value)
                {
                    case DownloadState.LocallyAvailable:
                        shakeContainer.Shake();
                        break;

                    default:
                        saveScore();
                        break;
                }
            };
            State.BindValueChanged(updateTooltip, true);
        }

        private void saveScore()
        {
            if (State.Value != DownloadState.LocallyAvailable)
                OnSave?.Invoke();

            State.Value = DownloadState.LocallyAvailable;
            button.State.Value = DownloadState.LocallyAvailable;
        }

        private void updateTooltip(ValueChangedEvent<DownloadState> state)
        {
            switch (state.NewValue)
            {
                case DownloadState.LocallyAvailable:
                    button.TooltipText = @"Score saved";
                    break;

                default:
                    button.TooltipText = @"Save score";
                    break;
            }
        }
    }
}
