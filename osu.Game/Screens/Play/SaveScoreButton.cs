using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;

namespace osu.Game.Screens.Play
{
    public class SaveScoreButton : DownloadButton
    {
        public Action OnSave;

        [BackgroundDependencyLoader]
        private void load()
        {
            State.BindValueChanged(updateTooltip, true);
            Action = saveScore;
        }

        private void saveScore()
        {
            if (State.Value != DownloadState.LocallyAvailable)
                OnSave?.Invoke();

            State.Value = DownloadState.LocallyAvailable;
        }

        private void updateTooltip(ValueChangedEvent<DownloadState> state)
        {
            switch (state.NewValue)
            {
                case DownloadState.LocallyAvailable:
                    TooltipText = @"Score saved";
                    break;

                default:
                    TooltipText = @"Save score";
                    break;
            }
        }
    }
}
