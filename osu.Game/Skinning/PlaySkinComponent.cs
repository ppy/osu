// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Skinning
{
    public class PlaySkinComponent<T> : ISkinComponent where T : struct
    {
        public readonly T Component;

        public PlaySkinComponent(T component)
        {
            this.Component = component;
        }

        protected virtual string RulesetPrefix => string.Empty;
        protected virtual string ComponentName => Component.ToString();

        public string LookupName =>
            string.Join("/", new[] { "Play", RulesetPrefix, ComponentName }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
