// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public static class HitObjectCommandProxy
    {
        public static double StartTime<T>(this CommandProxy<T> proxy) where T : HitObject => proxy.Target.StartTime;

        public static void SetStartTime<T>(this CommandProxy<T> proxy, double value) where T : HitObject => proxy.Submit(new SetStartTimeCommand(proxy.Target, value));

        public static CommandProxy<SliderPath> Path<T>(this CommandProxy<T> proxy) where T : IHasPath =>
            new CommandProxy<SliderPath>(proxy.CommandHandler, proxy.Target.Path);

        public static double SliderVelocityMultiplier<T>(this CommandProxy<T> proxy) where T : IHasSliderVelocity => proxy.Target.SliderVelocityMultiplier;

        public static void SetSliderVelocityMultiplier<T>(this CommandProxy<T> proxy, double value) where T : IHasSliderVelocity =>
            proxy.Submit(new SetSliderVelocityMultiplierCommand(proxy.Target, value));
    }
}
