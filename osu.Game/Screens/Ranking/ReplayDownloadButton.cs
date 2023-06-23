﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class ReplayDownloadButton : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        public readonly Bindable<ScoreInfo> Score = new Bindable<ScoreInfo>();

        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        private DownloadButton button = null!;
        private ShakeContainer shakeContainer = null!;

        private ScoreDownloadTracker? downloadTracker;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        private ReplayAvailability replayAvailability
        {
            get
            {
                if (State.Value == DownloadState.LocallyAvailable)
                    return ReplayAvailability.Local;

                if (Score.Value?.HasReplay == true)
                    return ReplayAvailability.Online;

                return ReplayAvailability.NotAvailable;
            }
        }

        public ReplayDownloadButton(ScoreInfo score)
        {
            Score.Value = score;
            Size = new Vector2(50, 30);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame? game, ScoreModelDownloader scoreDownloader)
        {
            InternalChild = shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new DownloadButton
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            button.Action = () =>
            {
                switch (State.Value)
                {
                    case DownloadState.LocallyAvailable:
                        game?.PresentScore(Score.Value, ScorePresentType.Gameplay);
                        break;

                    case DownloadState.NotDownloaded:
                        scoreDownloader.Download(Score.Value);
                        break;

                    case DownloadState.Importing:
                    case DownloadState.Downloading:
                        shakeContainer.Shake();
                        break;
                }
            };

            Score.BindValueChanged(score =>
            {
                downloadTracker?.RemoveAndDisposeImmediately();

                if (score.NewValue != null)
                {
                    AddInternal(downloadTracker = new ScoreDownloadTracker(score.NewValue)
                    {
                        State = { BindTarget = State }
                    });
                }

                updateState();
            }, true);

            State.BindValueChanged(state =>
            {
                button.State.Value = state.NewValue;
                updateState();
            }, true);
        }

        #region Export via hotkey logic (also in SaveFailedScoreButton)

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.SaveReplay:
                    button.TriggerClick();
                    return true;

                case GlobalAction.ExportReplay:
                    State.BindValueChanged(exportWhenReady, true);

                    // start the import via button
                    if (State.Value != DownloadState.LocallyAvailable)
                        button.TriggerClick();

                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void exportWhenReady(ValueChangedEvent<DownloadState> state)
        {
            if (state.NewValue != DownloadState.LocallyAvailable) return;

            scoreManager.Export(Score.Value);

            State.ValueChanged -= exportWhenReady;
        }

        #endregion

        private void updateState()
        {
            switch (replayAvailability)
            {
                case ReplayAvailability.Local:
                    button.TooltipText = @"watch replay";
                    button.Enabled.Value = true;
                    break;

                case ReplayAvailability.Online:
                    button.TooltipText = @"download replay";
                    button.Enabled.Value = true;
                    break;

                default:
                    button.TooltipText = @"replay unavailable";
                    button.Enabled.Value = false;
                    break;
            }
        }

        private enum ReplayAvailability
        {
            Local,
            Online,
            NotAvailable,
        }
    }
}
