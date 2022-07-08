// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
        public Func<Task<ScoreInfo>> ImportFailedScore;
        private Task<ScoreInfo> saveFailedScoreTask;
        private ScoreInfo score;

        protected readonly Bindable<ImportState> State = new Bindable<ImportState>();

        private DownloadButton button;

        public SaveFailedScoreButton(Func<Task<ScoreInfo>> requestImportFailedScore)
        {
            Size = new Vector2(50, 30);
            ImportFailedScore = requestImportFailedScore;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game)
        {
            InternalChild = button = new DownloadButton
            {
                RelativeSizeAxes = Axes.Both,
            };

            button.Action = () =>
            {
                switch (State.Value)
                {
                    case ImportState.Imported:
                        game?.PresentScore(score, ScorePresentType.Gameplay);
                        break;

                    case ImportState.Importing:
                        break;

                    default:
                        saveScore();
                        break;
                }
            };
            State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case ImportState.Imported:
                        button.State.Value = DownloadState.LocallyAvailable;
                        break;

                    case ImportState.Importing:
                        button.State.Value = DownloadState.Importing;
                        break;

                    case ImportState.Failed:
                        button.State.Value = DownloadState.NotDownloaded;
                        break;
                }
            }, true);
            State.BindValueChanged(updateState, true);
        }

        private void saveScore()
        {
            if (saveFailedScoreTask != null)
            {
                return;
            }

            State.Value = ImportState.Importing;

            saveFailedScoreTask = Task.Run(ImportFailedScore);
            saveFailedScoreTask.ContinueWith(s => Schedule(() =>
            {
                score = s.GetAwaiter().GetResult();
                State.Value = score != null ? ImportState.Imported : ImportState.Failed;
            }));
        }

        private void updateState(ValueChangedEvent<ImportState> state)
        {
            switch (state.NewValue)
            {
                case ImportState.Imported:
                    button.TooltipText = @"Watch replay";
                    button.Enabled.Value = true;
                    break;

                case ImportState.Importing:
                    button.TooltipText = @"Importing score";
                    button.Enabled.Value = false;
                    break;

                case ImportState.Failed:
                    button.TooltipText = @"Import failed, click button to re-import";
                    button.Enabled.Value = true;
                    break;

                default:
                    button.TooltipText = @"Save score";
                    button.Enabled.Value = true;
                    break;
            }
        }

        public enum ImportState
        {
            NotImported,
            Failed,
            Importing,
            Imported
        }
    }
}
