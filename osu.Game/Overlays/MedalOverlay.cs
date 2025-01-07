// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.Notifications.WebSocket;
using osu.Game.Online.Notifications.WebSocket.Events;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public partial class MedalOverlay : OsuFocusedOverlayContainer
    {
        protected override string? PopInSampleName => null;
        protected override string? PopOutSampleName => null;

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        protected override void PopIn() => this.FadeIn();

        protected override void PopOut() => this.FadeOut();

        private readonly Queue<MedalAnimation> queuedMedals = new Queue<MedalAnimation>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Container<Drawable> medalContainer = null!;
        private MedalAnimation? currentMedalDisplay;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            api.NotificationsClient.MessageReceived += handleMedalMessages;

            Add(medalContainer = new Container
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            OverlayActivationMode.BindValueChanged(_ => showNextMedal(), true);
        }

        public override void Hide()
        {
            // don't allow hiding the overlay via any method other than our own.
        }

        private void handleMedalMessages(SocketMessage obj)
        {
            if (obj.Event != @"new")
                return;

            var data = obj.Data?.ToObject<NewPrivateNotificationEvent>();
            if (data == null || data.Name != @"user_achievement_unlock")
                return;

            var details = data.Details?.ToObject<UserAchievementUnlock>();
            if (details == null)
                return;

            var medal = new Medal
            {
                Name = details.Title,
                InternalName = details.Slug,
                Description = details.Description,
            };

            var medalAnimation = new MedalAnimation(medal);

            Logger.Log($"Queueing medal unlock for \"{medal.Name}\" ({queuedMedals.Count} to display)");

            Schedule(() => LoadComponentAsync(medalAnimation, m =>
            {
                queuedMedals.Enqueue(m);
                showNextMedal();
            }));
        }

        protected override bool OnClick(ClickEvent e)
        {
            progressDisplayByUser();
            return true;
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Back)
            {
                progressDisplayByUser();
                return true;
            }

            return base.OnPressed(e);
        }

        private void progressDisplayByUser()
        {
            // Dismissing may sometimes play out the medal animation rather than immediately dismissing.
            if (currentMedalDisplay?.Dismiss() == false)
                return;

            currentMedalDisplay = null;
            showNextMedal();
        }

        private void showNextMedal()
        {
            // If already displayed, keep displaying medals regardless of activation mode changes.
            if (OverlayActivationMode.Value != OverlayActivation.All && State.Value == Visibility.Hidden)
                return;

            // A medal is already displaying.
            if (currentMedalDisplay != null)
                return;

            if (queuedMedals.TryDequeue(out currentMedalDisplay))
            {
                Logger.Log($"Displaying \"{currentMedalDisplay.Medal.Name}\"");
                medalContainer.Add(currentMedalDisplay);
                Show();
            }
            else if (State.Value == Visibility.Visible)
            {
                Logger.Log("All queued medals have been displayed, hiding overlay!");
                base.Hide();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            // this event subscription fires async loads, which hard-fail if `CompositeDrawable.disposalCancellationSource` is canceled, which happens in the base call.
            // therefore, unsubscribe from this event early to reduce the chances of a stray event firing at an inconvenient spot.
            if (api.IsNotNull())
                api.NotificationsClient.MessageReceived -= handleMedalMessages;

            base.Dispose(isDisposing);
        }
    }
}
