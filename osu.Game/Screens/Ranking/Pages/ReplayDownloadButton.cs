// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Pages
{
    public class ReplayDownloadButton : DownloadTrackingComposite<ScoreInfo, ScoreManager>
    {
        private DownloadButton button;
        private ShakeContainer shakeContainer;

        private ReplayAvailability replayAvailability
        {
            get
            {
                if (State.Value == DownloadState.LocallyAvailable)
                    return ReplayAvailability.Local;

                if (Model.Value is APILegacyScoreInfo apiScore && apiScore.Replay)
                    return ReplayAvailability.Online;

                return ReplayAvailability.NotAvailable;
            }
        }

        public ReplayDownloadButton(ScoreInfo score)
            : base(score)
        {
            Size = new Vector2(50, 30);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, ScoreManager scores)
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
                        game?.PresentScore(Model.Value);
                        break;

                    case DownloadState.NotDownloaded:
                        scores.Download(Model.Value);
                        break;

                    case DownloadState.Downloaded:
                    case DownloadState.Downloading:
                        shakeContainer.Shake();
                        break;
                }
            };

            State.BindValueChanged(state =>
            {
                button.State.Value = state.NewValue;

                switch (replayAvailability)
                {
                    case ReplayAvailability.Local:
                        button.TooltipText = @"Watch replay";
                        break;

                    case ReplayAvailability.Online:
                        button.TooltipText = @"Download replay";
                        break;

                    default:
                        button.TooltipText = @"Replay unavailable";
                        break;
                }
            }, true);

            button.Enabled.Value = replayAvailability != ReplayAvailability.NotAvailable;
        }

        private enum ReplayAvailability
        {
            Local,
            Online,
            NotAvailable,
        }
    }
}
