// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for any mode that changes the behaviour of the cursor, for example, making it lag behind.
    /// </summary>
    public interface IModifiesCursorMovement
    {
        public Vector2 UpdatePosition(Vector2 CursorPosition, float deltaTime);
    }
}
