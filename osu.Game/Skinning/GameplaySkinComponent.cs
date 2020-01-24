// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Skinning
{
    public class GameplaySkinComponent<T> : ISkinComponent
    {
        public readonly T Component;

        public GameplaySkinComponent(T component)
        {
            Component = component;
        }

        protected virtual string RulesetPrefix => string.Empty;
        protected virtual string ComponentName => Component.ToString();

        public string LookupName =>
            string.Join("/", new[] { "Gameplay", RulesetPrefix, ComponentName }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
