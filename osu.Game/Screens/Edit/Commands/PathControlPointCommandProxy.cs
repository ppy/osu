// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Commands
{
    public static class PathControlPointCommandProxy
    {
        public static Vector2 Position<T>(this CommandProxy<T> proxy) where T : PathControlPoint => proxy.Target.Position;

        public static void SetPosition(this CommandProxy<PathControlPoint> proxy, Vector2 value) => proxy.Submit(new UpdateControlPointCommand(proxy.Target) { Position = value });

        public static PathType? Type<T>(this CommandProxy<T> proxy) where T : PathControlPoint => proxy.Target.Type;

        public static void SetType(this CommandProxy<PathControlPoint> proxy, PathType? value) => proxy.Submit(new UpdateControlPointCommand(proxy.Target) { Type = value });
    }
}
