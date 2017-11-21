// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Notifications
{
    public class ProgressNotification : Notification, IHasCompletionTarget
    {
        public float Progress
        {
            get { return ProgressBinding.Value; }
            set { ProgressBinding.Value = value; }
        }
        public ProgressNotificationState State
        {
            get { return StateBinding.Value; }
            set { StateBinding.Value = value; }
        }

        public Bindable<float> ProgressBinding { get; }
        public Bindable<ProgressNotificationState> StateBinding { get; }

        public event Action ProgressCompleted;
        public event Action CancelRequested;

        public bool IsCompleted => ProgressBinding.Value >= 1;

        public ProgressNotification(string text, FontAwesome icon = FontAwesome.fa_info_circle)
         : base(text, icon)
        {
            ProgressBinding = new BindableFloat();
            ProgressBinding.ValueChanged += progressOnValueChanged;

            StateBinding = new Bindable<ProgressNotificationState>();
            ProgressCompleted += () => FollowUpNotifications.ForEach(followUpNotification => CompletionTarget.Invoke(followUpNotification));
        }

        public void RequestCancel()
        {
            CancelRequested?.Invoke();
        }

        private void progressOnValueChanged(float newValue)
        {
            if (IsCompleted)
                ProgressCompleted?.Invoke();
        }


        public List<NotificationContainer> FollowUpNotifications { get; } = new List<NotificationContainer>();
        public Action<NotificationContainer> CompletionTarget { get; set; }
    }
}
