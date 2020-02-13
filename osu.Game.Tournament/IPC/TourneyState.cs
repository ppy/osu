// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Tournament.IPC
{
    public enum TourneyState
    {
        Initialising,
        Idle,
        WaitingForClients,
        Playing,
        Ranking
    }
}
