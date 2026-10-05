// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    public partial class StageDisplay
    {
        public partial class StatusText : CompositeDrawable
        {
            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private OsuSpriteText text = null!;

            private Sample? textChangedSample;
            private double? lastSamplePlayback;

            public StatusText()
            {
                AutoSizeAxes = Axes.X;
                Height = 16;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                InternalChild = text = new OsuSpriteText
                {
                    Alpha = 0,
                    Height = 16,
                    Font = OsuFont.Style.Caption1,
                    AlwaysPresent = true,
                };

                textChangedSample = audio.Samples.Get(@"Multiplayer/Matchmaking/stage-message");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                client.MatchRoomStateChanged += onMatchRoomStateChanged;
                onMatchRoomStateChanged(client.Room!.MatchState);
            }

            private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
            {
                if (state is not MatchmakingRoomState matchmakingState)
                    return;

                text.Text = getTextForStatus(matchmakingState.Stage);

                if (text.Text == string.Empty || (lastSamplePlayback != null && Time.Current - lastSamplePlayback < OsuGameBase.SAMPLE_DEBOUNCE_TIME))
                    return;

                if (matchmakingState.Stage is MatchmakingStage.WaitingForClientsJoin or MatchmakingStage.WaitingForClientsBeatmapDownload)
                {
                    textChangedSample?.Play();
                    lastSamplePlayback = Time.Current;
                }

                LocalisableString textForStatus = getTextForStatus(matchmakingState.Stage);

                if (string.IsNullOrEmpty(textForStatus.ToString()))
                {
                    text.FadeOut();
                    return;
                }

                text.RotateTo(2f)
                    .RotateTo(0, 500, Easing.OutQuint);

                text.FadeInFromZero(500, Easing.OutQuint);

                using (text.BeginDelayedSequence(500))
                {
                    text
                        .FadeTo(0.6f, 400, Easing.In)
                        .Then()
                        .FadeTo(1, 400, Easing.Out)
                        .Loop();
                }

                text.ScaleTo(0.3f)
                    .ScaleTo(1, 500, Easing.OutQuint);

                text.Text = textForStatus;
            });

            private LocalisableString getTextForStatus(MatchmakingStage status)
            {
                switch (status)
                {
                    case MatchmakingStage.WaitingForClientsJoin:
                        return "Players are joining the match...";

                    case MatchmakingStage.WaitingForClientsBeatmapDownload:
                        return "Players are downloading the beatmap...";

                    case MatchmakingStage.Gameplay:
                        return "Game is in progress...";

                    case MatchmakingStage.Ended:
                        return "Thanks for playing! The match will close shortly.";

                    default:
                        return string.Empty;
                }
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (client.IsNotNull())
                    client.MatchRoomStateChanged -= onMatchRoomStateChanged;
            }
        }
    }
}
