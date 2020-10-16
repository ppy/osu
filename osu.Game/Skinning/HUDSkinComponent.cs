// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Skinning
{
    public class HUDSkinComponent : ISkinComponent
    {
        public readonly HUDSkinComponents Component;

        public HUDSkinComponent(HUDSkinComponents component)
        {
            Component = component;
        }

        protected virtual string ComponentName => Component.ToString();

        public string LookupName =>
            string.Join('/', new[] { "HUD", ComponentName }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
