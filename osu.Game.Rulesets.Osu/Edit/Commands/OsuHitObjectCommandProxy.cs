// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Commands;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public static class OsuHitObjectCommandProxy
    {
        public static Vector2 Position<T>(this CommandProxy<T> proxy) where T : OsuHitObject => proxy.Target.Position;

        public static void SetPosition<T>(this CommandProxy<T> proxy, Vector2 value) where T : OsuHitObject =>
            proxy.Submit(new SetPositionCommand(proxy.Target, value));

        public static bool NewCombo<T>(this CommandProxy<T> proxy) where T : OsuHitObject => proxy.Target.NewCombo;

        public static void SetNewCombo<T>(this CommandProxy<T> proxy, bool value) where T : OsuHitObject =>
            proxy.Submit(new SetNewComboCommand(proxy.Target, value));
    }
}
