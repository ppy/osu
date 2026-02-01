// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace osu.Desktop.GameState
{
    public class GameStateHub : Hub<IGameStateClient>;

    public interface IGameStateClient
    {
        Task SoloGameplayStarted(int ruleset, string name);

        Task SoloGameplayEnded();
    }
}
