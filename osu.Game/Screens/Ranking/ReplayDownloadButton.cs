﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class ReplayDownloadButton : CompositeDrawable
    {
        public readonly Bindable<ScoreInfo> Score = new Bindable<ScoreInfo>();

        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        private DownloadButton button;
        private ShakeContainer shakeContainer;

        private ScoreDownloadTracker downloadTracker;

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

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, ScoreModelDownloader scores)
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
                        scores.Download(Score.Value);
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
