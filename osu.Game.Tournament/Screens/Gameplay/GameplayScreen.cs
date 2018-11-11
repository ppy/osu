// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public class GameplayScreen : BeatmapInfoScreen
    {
        private readonly BindableBool warmup = new BindableBool();

        private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();

        public readonly Bindable<TourneyState> State = new Bindable<TourneyState>();
        private TriangleButton warmupButton;
        private FileBasedIPC ipc;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, TextureStore textures, FileBasedIPC ipc)
        {
            this.ipc = ipc;
            AddRange(new Drawable[]
            {
                new MatchHeader(),
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 718 / 1080f,
                    Colour = new Color4(0, 255, 0, 255),
                    Y = 14,
                    Anchor = Anchor.Centre,
                    Origin= Anchor.Centre,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        warmupButton = new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle warmup",
                            Action = () => warmup.Toggle()
                        }
                    }
                }
            });

            State.BindValueChanged(stateChanged);
            State.BindTo(ipc.State);

            currentMatch.BindValueChanged(m => warmup.Value = m.Team1Score + m.Team2Score == 0);
            currentMatch.BindTo(ladder.CurrentMatch);

            warmup.BindValueChanged(w => warmupButton.Alpha = !w ? 0.5f : 1, true);
        }

        private ScheduledDelegate scheduledBarContract;

        private void stateChanged(TourneyState state)
        {
            if (state == TourneyState.Ranking)
            {
                if (warmup.Value) return;

                if (ipc.Score1 > ipc.Score2)
                    currentMatch.Value.Team1Score.Value++;
                else
                    currentMatch.Value.Team2Score.Value++;
            }

            scheduledBarContract?.Cancel();

            switch (state)
            {
                case TourneyState.Idle:
                    // show chat
                    SongBar.Expanded = false;
                    break;
                case TourneyState.Ranking:
                    scheduledBarContract = Scheduler.AddDelayed(() => SongBar.Expanded = false, 15000);
                    break;
                default:
                    SongBar.Expanded = true;
                    break;
            }
        }
    }
}
