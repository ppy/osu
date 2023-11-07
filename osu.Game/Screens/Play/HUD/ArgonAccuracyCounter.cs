// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonAccuracyCounter : GameplayAccuracyCounter, ISerialisableDrawable
    {
        [SettingSource("Wireframe opacity", "Controls the opacity of the wire frames behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.4f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        public bool UsesFixedAnchor { get; set; }

        protected override IHasText CreateText() => new ArgonCounterTextComponent(Anchor.TopLeft, "ACCURACY", new Vector2(-4, 0))
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
        };
    }
}
