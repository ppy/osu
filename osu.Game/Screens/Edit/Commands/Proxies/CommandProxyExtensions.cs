// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Commands.Proxies
{
    public static class CommandProxyExtensions
    {
        #region Position

        public static Vector2 Position<T>(this CommandProxy<T> proxy) where T : IHasPosition => proxy.Target.Position;

        public static void SetPosition<T>(this CommandProxy<T> proxy, Vector2 value) where T : IHasMutablePosition => proxy.Submit(new SetPositionCommand(proxy.Target, value));

        public static float X<T>(this CommandProxy<T> proxy) where T : IHasXPosition => proxy.Target.X;

        public static void SetX<T>(this CommandProxy<T> proxy, float value) where T : IHasMutableXPosition => proxy.Submit(new SetXCommand(proxy.Target, value));

        public static float Y<T>(this CommandProxy<T> proxy) where T : IHasYPosition => proxy.Target.Y;

        public static void SetY<T>(this CommandProxy<T> proxy, float value) where T : IHasMutableYPosition => proxy.Submit(new SetYCommand(proxy.Target, value));

        #endregion

        #region HitObject

        public static double StartTime<T>(this CommandProxy<T> proxy) where T : HitObject => proxy.Target.StartTime;

        public static void SetStartTime<T>(this CommandProxy<T> proxy, double value) where T : HitObject => proxy.Submit(new SetStartTimeCommand(proxy.Target, value));

        public static bool NewCombo<T>(this CommandProxy<T> proxy) where T : IHasCombo => proxy.Target.NewCombo;

        public static void SetNewCombo<T>(this CommandProxy<T> proxy, bool value) where T : IHasComboInformation => proxy.Submit(new SetNewComboCommand(proxy.Target, value));

        public static double SliderVelocityMultiplier<T>(this CommandProxy<T> proxy) where T : IHasSliderVelocity => proxy.Target.SliderVelocityMultiplier;

        public static void SetSliderVelocityMultiplier<T>(this CommandProxy<T> proxy, double value) where T : IHasSliderVelocity =>
            proxy.Submit(new SetSliderVelocityMultiplierCommand(proxy.Target, value));

        #endregion

        #region PathControlPoint

        public static PathType? Type<T>(this CommandProxy<T> proxy) where T : PathControlPoint => proxy.Target.Type;

        public static void SetType(this CommandProxy<PathControlPoint> proxy, PathType? value) => proxy.Submit(new SetPathTypeCommand(proxy.Target, value));

        #endregion

        #region SliderPath

        public static CommandProxy<SliderPath> Path<T>(this CommandProxy<T> proxy) where T : IHasPath =>
            new CommandProxy<SliderPath>(proxy.CommandHandler, proxy.Target.Path);

        public static double? ExpectedDistance(this CommandProxy<SliderPath> proxy) => proxy.Target.ExpectedDistance.Value;

        public static void SetExpectedDistance(this CommandProxy<SliderPath> proxy, double? value) => proxy.Submit(new SetExpectedDistanceCommand(proxy.Target, value));

        public static ListCommandProxy<PathControlPoint, CommandProxy<PathControlPoint>> ControlPoints(this CommandProxy<SliderPath> proxy) =>
            new ListCommandProxy<PathControlPoint, CommandProxy<PathControlPoint>>(proxy.CommandHandler, proxy.Target.ControlPoints);

        public static IEnumerable<double> GetSegmentEnds(this ICommandProxy<SliderPath> proxy) => proxy.Target.GetSegmentEnds();

        #endregion
    }
}
