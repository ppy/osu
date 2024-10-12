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
        public static ListCommandProxy<TItem, CommandProxy<TItem>> AsListCommandProxy<TItem>(this IList<TItem> target, EditorCommandHandler? commandHandler) =>
            new ListCommandProxy<TItem, CommandProxy<TItem>>(commandHandler, target);

        public static CommandProxy<T> AsCommandProxy<T>(this T target, EditorCommandHandler? commandHandler) => new CommandProxy<T>(commandHandler, target);

        public static CommandProxy<T> Submit<T>(this CommandProxy<T> proxy, IEditorCommand command)
        {
            proxy.CommandHandler?.SafeSubmit(command);

            return proxy;
        }

        public static CommandProxy<T> BeginChange<T>(this CommandProxy<T> proxy)
        {
            proxy.CommandHandler?.BeginChange();

            return proxy;
        }

        public static CommandProxy<T> EndChange<T>(this CommandProxy<T> proxy)
        {
            proxy.CommandHandler?.EndChange();

            return proxy;
        }

        #region Position

        public static Vector2 Position<T>(this CommandProxy<T> proxy) where T : IHasPosition => proxy.Target.Position;

        public static CommandProxy<T> SetPosition<T>(this CommandProxy<T> proxy, Vector2 value) where T : IHasMutablePosition => proxy.Submit(new SetPositionCommand(proxy.Target, value));

        public static float X<T>(this CommandProxy<T> proxy) where T : IHasXPosition => proxy.Target.X;

        public static CommandProxy<T> SetX<T>(this CommandProxy<T> proxy, float value) where T : IHasMutableXPosition => proxy.Submit(new SetXCommand(proxy.Target, value));

        public static float Y<T>(this CommandProxy<T> proxy) where T : IHasYPosition => proxy.Target.Y;

        public static CommandProxy<T> SetY<T>(this CommandProxy<T> proxy, float value) where T : IHasMutableYPosition => proxy.Submit(new SetYCommand(proxy.Target, value));

        #endregion

        #region HitObject

        public static double StartTime<T>(this CommandProxy<T> proxy) where T : HitObject => proxy.Target.StartTime;

        public static CommandProxy<T> SetStartTime<T>(this CommandProxy<T> proxy, double value) where T : HitObject => proxy.Submit(new SetStartTimeCommand(proxy.Target, value));

        public static bool NewCombo<T>(this CommandProxy<T> proxy) where T : IHasCombo => proxy.Target.NewCombo;

        public static CommandProxy<T> SetNewCombo<T>(this CommandProxy<T> proxy, bool value) where T : IHasComboInformation => proxy.Submit(new SetNewComboCommand(proxy.Target, value));

        public static double SliderVelocityMultiplier<T>(this CommandProxy<T> proxy) where T : IHasSliderVelocity => proxy.Target.SliderVelocityMultiplier;

        public static CommandProxy<T> SetSliderVelocityMultiplier<T>(this CommandProxy<T> proxy, double value) where T : IHasSliderVelocity =>
            proxy.Submit(new SetSliderVelocityMultiplierCommand(proxy.Target, value));

        #endregion

        #region PathControlPoint

        public static PathType? Type<T>(this CommandProxy<T> proxy) where T : PathControlPoint => proxy.Target.Type;

        public static CommandProxy<PathControlPoint> SetType(this CommandProxy<PathControlPoint> proxy, PathType? value) => proxy.Submit(new SetPathTypeCommand(proxy.Target, value));

        #endregion

        #region SliderPath

        public static CommandProxy<SliderPath> Path<T>(this CommandProxy<T> proxy) where T : IHasPath =>
            new CommandProxy<SliderPath>(proxy.CommandHandler, proxy.Target.Path);

        public static CommandProxy<T> RemoveControlPoint<T>(this CommandProxy<T> proxy, PathControlPoint controlPoint) where T : IHasPath =>
            proxy.Submit(new ListCommands.Remove<PathControlPoint>(proxy.Target.Path.ControlPoints, controlPoint));

        public static CommandProxy<T> RemoveControlPointAt<T>(this CommandProxy<T> proxy, int index) where T : IHasPath =>
            proxy.Submit(new ListCommands.Remove<PathControlPoint>(proxy.Target.Path.ControlPoints, index));

        public static CommandProxy<T> InsertControlPointAt<T>(this CommandProxy<T> proxy, int index, PathControlPoint controlPoint) where T : IHasPath =>
            proxy.Submit(new ListCommands.Insert<PathControlPoint>(proxy.Target.Path.ControlPoints, index, controlPoint));

        public static double? ExpectedDistance(this CommandProxy<SliderPath> proxy) => proxy.Target.ExpectedDistance.Value;

        public static CommandProxy<SliderPath> SetExpectedDistance(this CommandProxy<SliderPath> proxy, double? value) => proxy.Submit(new SetExpectedDistanceCommand(proxy.Target, value));

        public static ListCommandProxy<PathControlPoint, CommandProxy<PathControlPoint>> ControlPoints(this CommandProxy<SliderPath> proxy) =>
            new ListCommandProxy<PathControlPoint, CommandProxy<PathControlPoint>>(proxy.CommandHandler, proxy.Target.ControlPoints);

        public static IEnumerable<double> GetSegmentEnds(this ICommandProxy<SliderPath> proxy) => proxy.Target.GetSegmentEnds();

        #endregion

        #region EditorBeatmap

        public static CommandProxy<EditorBeatmap> Add(this CommandProxy<EditorBeatmap> proxy, HitObject hitObject) =>
            proxy.Submit(new AddHitObjectCommand(proxy.Target, hitObject));

        public static CommandProxy<EditorBeatmap> Remove(this CommandProxy<EditorBeatmap> proxy, HitObject hitObject) =>
            proxy.Submit(new RemoveHitObjectCommand(proxy.Target, hitObject));

        #endregion
    }
}
