// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Scoring;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class SaveFailedScoreButton : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        private readonly Func<Task<ScoreInfo>>? importFailedScore;

        private Live<ScoreInfo>? importedScore;

        private DownloadButton button = null!;

        public SaveFailedScoreButton(Func<Task<ScoreInfo>>? importFailedScore)
        {
            Size = new Vector2(50, 30);

            this.importFailedScore = importFailedScore;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame? game, Player? player)
        {
            InternalChild = button = new DownloadButton
            {
                RelativeSizeAxes = Axes.Both,
                State = { BindTarget = state },
                Action = () =>
                {
                    switch (state.Value)
                    {
                        case DownloadState.LocallyAvailable:
                            game?.PresentScore(importedScore?.Value, ScorePresentType.Gameplay);
                            break;

                        case DownloadState.NotDownloaded:
                            state.Value = DownloadState.Importing;

                            if (importFailedScore != null)
                            {
                                Task.Run(importFailedScore).ContinueWith(t =>
                                {
                                    importedScore = realm.Run<Live<ScoreInfo>?>(r => r.Find<ScoreInfo>(t.GetResultSafely().ID)?.ToLive(realm));
                                    Schedule(() => state.Value = importedScore != null ? DownloadState.LocallyAvailable : DownloadState.NotDownloaded);
                                }).FireAndForget();
                            }

                            break;
                    }
                }
            };

            if (player != null)
            {
                importedScore = realm.Run(r => r.Find<ScoreInfo>(player.Score.ScoreInfo.ID)?.ToLive(realm));
                state.Value = importedScore != null ? DownloadState.LocallyAvailable : DownloadState.NotDownloaded;
            }

            state.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.LocallyAvailable:
                        button.TooltipText = @"watch replay";
                        button.Enabled.Value = true;
                        break;

                    case DownloadState.Importing:
                        button.TooltipText = @"importing score";
                        button.Enabled.Value = false;
                        break;

                    default:
                        if (importFailedScore != null)
                        {
                            button.TooltipText = @"save score";
                            button.Enabled.Value = true;
                        }
                        else
                        {
                            button.TooltipText = @"replay unavailable";
                            button.Enabled.Value = false;
                        }

                        break;
                }
            }, true);
        }

        #region Export via hotkey logic (also in ReplayDownloadButton)

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.SaveReplay:
                    button.TriggerClick();
                    return true;

                case GlobalAction.ExportReplay:
                    state.BindValueChanged(exportWhenReady, true);

                    // start the import via button
                    if (state.Value != DownloadState.LocallyAvailable)
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

            if (importedScore != null) scoreManager.Export(importedScore.Value);

            this.state.ValueChanged -= exportWhenReady;
        }

        #endregion
    }
}
