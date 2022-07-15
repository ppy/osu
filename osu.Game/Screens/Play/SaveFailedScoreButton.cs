// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osuTK;

namespace osu.Game.Screens.Play
{
    public class SaveFailedScoreButton : CompositeDrawable
    {
        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        private readonly Func<Task<ScoreInfo>> importFailedScore;

        private Task<ScoreInfo>? saveFailedScoreTask;

        private ScoreInfo? score;

        private DownloadButton button = null!;

        public SaveFailedScoreButton(Func<Task<ScoreInfo>> requestImportFailedScore)
        {
            Size = new Vector2(50, 30);
            importFailedScore = requestImportFailedScore;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame? game)
        {
            InternalChild = button = new DownloadButton
            {
                RelativeSizeAxes = Axes.Both,
            };

            button.Action = () =>
            {
                switch (state.Value)
                {
                    case DownloadState.LocallyAvailable:
                        game?.PresentScore(score, ScorePresentType.Gameplay);
                        break;

                    case DownloadState.Importing:
                        break;

                    default:
                        saveScore();
                        break;
                }
            };
            state.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.LocallyAvailable:
                        button.State.Value = DownloadState.LocallyAvailable;
                        break;

                    case DownloadState.Importing:
                        button.State.Value = DownloadState.Importing;
                        break;

                    case DownloadState.NotDownloaded:
                        button.State.Value = DownloadState.NotDownloaded;
                        break;
                }
            }, true);
            state.BindValueChanged(updateState, true);
        }

        private void saveScore()
        {
            if (saveFailedScoreTask != null)
            {
                return;
            }

            state.Value = DownloadState.Importing;

            saveFailedScoreTask = Task.Run(importFailedScore);
            saveFailedScoreTask.ContinueWith(s => Schedule(() =>
            {
                score = s.GetAwaiter().GetResult();
                state.Value = score != null ? DownloadState.LocallyAvailable : DownloadState.NotDownloaded;
            }));
        }

        private void updateState(ValueChangedEvent<DownloadState> state)
        {
            switch (state.NewValue)
            {
                case DownloadState.LocallyAvailable:
                    button.TooltipText = @"Watch replay";
                    button.Enabled.Value = true;
                    break;

                case DownloadState.Importing:
                    button.TooltipText = @"Importing score";
                    button.Enabled.Value = false;
                    break;

                default:
                    button.TooltipText = @"Save score";
                    button.Enabled.Value = true;
                    break;
            }
        }
    }
}
