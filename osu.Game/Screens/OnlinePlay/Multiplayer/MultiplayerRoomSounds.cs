// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerRoomSounds : MultiplayerRoomComposite
    {
        private Sample hostChangedSample;
        private Sample userJoinedSample;
        private Sample userLeftSample;
        private Sample userKickedSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            hostChangedSample = audio.Samples.Get(@"Multiplayer/host-changed");
            userJoinedSample = audio.Samples.Get(@"Multiplayer/player-joined");
            userLeftSample = audio.Samples.Get(@"Multiplayer/player-left");
            userKickedSample = audio.Samples.Get(@"Multiplayer/player-kicked");

            Host.ValueChanged += hostChanged;
        }

        private void hostChanged(ValueChangedEvent<APIUser> value)
        {
            if (value.OldValue == null) return;

            hostChangedSample?.Play();
        }

        protected override void UserJoined(MultiplayerRoomUser user)
        {
            base.UserJoined(user);

            userJoinedSample?.Play();
        }

        protected override void UserLeft(MultiplayerRoomUser user)
        {
            base.UserLeft(user);

            userLeftSample?.Play();
        }

        protected override void UserKicked(MultiplayerRoomUser user)
        {
            base.UserKicked(user);

            userKickedSample?.Play();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Host.ValueChanged -= hostChanged;
        }
    }
}
