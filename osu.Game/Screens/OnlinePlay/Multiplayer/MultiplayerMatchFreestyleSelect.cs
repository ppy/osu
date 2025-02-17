// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerMatchFreestyleSelect : OnlinePlayFreestyleSelect
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private OngoingOperationTracker operationTracker { get; set; } = null!;

        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();

        private LoadingLayer loadingLayer = null!;
        private IDisposable? selectionOperation;

        public MultiplayerMatchFreestyleSelect(Room room, PlaylistItem item)
            : base(room, item)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer(true));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            operationInProgress.BindTo(operationTracker.InProgress);
            operationInProgress.BindValueChanged(_ => updateLoadingLayer(), true);
        }

        private void updateLoadingLayer()
        {
            if (operationInProgress.Value)
                loadingLayer.Show();
            else
                loadingLayer.Hide();
        }

        protected override bool OnStart()
        {
            if (operationInProgress.Value)
            {
                Logger.Log($"{nameof(OnStart)} aborted due to {nameof(operationInProgress)}");
                return false;
            }

            selectionOperation = operationTracker.BeginOperation();

            client.ChangeUserStyle(Beatmap.Value.BeatmapInfo.OnlineID, Ruleset.Value.OnlineID)
                  .FireAndForget(onSuccess: () =>
                  {
                      selectionOperation.Dispose();

                      Schedule(() =>
                      {
                          // If an error or server side trigger occurred this screen may have already exited by external means.
                          if (this.IsCurrentScreen())
                              this.Exit();
                      });
                  }, onError: _ =>
                  {
                      selectionOperation.Dispose();

                      Schedule(() =>
                      {
                          Carousel.AllowSelection = true;
                      });
                  });

            return true;
        }
    }
}
