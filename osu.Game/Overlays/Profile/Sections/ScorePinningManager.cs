// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class ScorePinningManager : Component
    {
        public event Action? PinsChanged;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private readonly Dictionary<long, bool> scorePinMap = new Dictionary<long, bool>();

        public bool CanPin(SoloScoreInfo score) => score.CurrentUserAttributes?.Pin != null && score.Passed;

        public bool IsPinned(SoloScoreInfo score)
        {
            if (!CanPin(score))
                return false;

            if (!scorePinMap.TryGetValue(score.OnlineID, out bool pinned))
            {
                var pinAttributes = score.CurrentUserAttributes!.Value.Pin!.Value;
                scorePinMap[score.OnlineID] = pinned = pinAttributes.IsPinned;
            }

            return pinned;
        }

        public void PinScore(SoloScoreInfo score)
        {
            if (!CanPin(score))
                throw new InvalidOperationException("Attempting to pin a score not belonging to the local user.");

            if (IsPinned(score))
                return;

            var request = new PinScoreRequest(score);

            request.Success += () =>
            {
                scorePinMap[score.OnlineID] = true;
                PinsChanged?.Invoke();
            };

            request.Failure += e =>
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = e.Message,
                    Icon = FontAwesome.Solid.Times,
                });
            };

            api.Queue(request);
        }

        public void UnpinScore(SoloScoreInfo score)
        {
            if (!CanPin(score))
                throw new InvalidOperationException("Attempting to pin a score not belonging to the local user.");

            if (!IsPinned(score))
                return;

            var request = new UnpinScoreRequest(score);

            request.Success += () =>
            {
                scorePinMap[score.OnlineID] = false;
                PinsChanged?.Invoke();
            };

            request.Failure += e =>
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = e.Message,
                    Icon = FontAwesome.Solid.Times,
                });
            };

            api.Queue(request);
        }

        public void Invalidate() => scorePinMap.Clear();
    }
}
