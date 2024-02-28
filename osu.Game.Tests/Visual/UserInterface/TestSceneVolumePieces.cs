// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.Volume;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneVolumePieces : OsuTestScene
    {
        protected override void LoadComplete()
        {
            VolumeMeter meter;
            MuteButton mute;
            Add(meter = new VolumeMeter("MASTER", 125, Color4.Green) { Position = new Vector2(10) });
            AddSliderStep("master volume", 0, 10, 0, i => meter.Bindable.Value = i * 0.1);

            Add(new VolumeMeter("BIG", 250, Color4.Red)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(10),
                Margin = new MarginPadding { Left = 250 },
            });

            Add(new VolumeMeter("SML", 125, Color4.Blue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(10),
                Margin = new MarginPadding { Right = 500 },
            });

            Add(mute = new MuteButton
            {
                Margin = new MarginPadding { Top = 200 }
            });

            AddToggleStep("mute", b => mute.Current.Value = b);
        }
    }
}
