// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Notifications
{
    public class ProgressNotification : Notification, IHasFollowUpNotifications
    {
        /// <summary>
        /// The progress of the notification ranging from 0.0 to 1.0
        /// </summary>
        public float Progress
        {
            get { return ProgressBinding.Value; }
            set { ProgressBinding.Value = value; }
        }

        /// <summary>
        /// The current state of the notification.
        /// </summary>
        public ProgressNotificationState State
        {
            get { return StateBinding.Value; }
            set { StateBinding.Value = value; }
        }

        public Bindable<float> ProgressBinding { get; } = new BindableFloat();
        public Bindable<ProgressNotificationState> StateBinding { get; } = new Bindable<ProgressNotificationState>();

        /// <summary>
        /// A list of notifications that get run after this noctification completes.
        /// </summary>
        public List<Notification> FollowUpNotifications { get; } = new List<Notification>();

        /// <summary>
        /// Event that gets triggererd when the progress completes.
        /// </summary>
        public event Action Completed;

        /// <summary>
        /// Event that gets called when the user requests canceling the progress.
        /// </summary>
        public event Action CancelRequested;

        /// <summary>
        /// Represents the completion condition.
        /// </summary>
        public bool IsCompleted => ProgressBinding.Value >= 1;

        public ProgressNotification(string text = "", FontAwesome icon = FontAwesome.fa_info_circle)
          : base(text, icon)
        {
            ProgressBinding.ValueChanged += progressChanged;
        }

        /// <summary>
        /// Requests the progress to be canceled.
        /// </summary>
        public void RequestCancel()
        {
            CancelRequested?.Invoke();
        }

        private void progressChanged(float newValue)
        {
            if (IsCompleted)
                Completed?.Invoke();
        }
    }
}
