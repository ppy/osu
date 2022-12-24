// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Skinning
{
    public class GameplaySkinComponentLookup<T> : ISkinComponentLookup
        where T : notnull
    {
        public readonly T Component;

        public GameplaySkinComponentLookup(T component)
        {
            Component = component;
        }

        protected virtual string RulesetPrefix => string.Empty;
        protected virtual string ComponentName => Component.ToString() ?? string.Empty;

        public string LookupName =>
            string.Join('/', new[] { "Gameplay", RulesetPrefix, ComponentName }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
