// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// User specific state for the current match ruleset.
    /// Can be used to contain any state which should be used before or during match gameplay.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public abstract class MatchRulesetUserState
    {
    }
}
