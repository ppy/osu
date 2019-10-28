// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing
{
    internal class EffectSection : Section<EffectControlPoint>
    {
        private OsuSpriteText kiai;
        private OsuSpriteText omitBarLine;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new[]
            {
                kiai = new OsuSpriteText(),
                omitBarLine = new OsuSpriteText(),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ControlPoint.BindValueChanged(point =>
            {
                kiai.Text = $"Kiai: {(point.NewValue?.KiaiMode == true ? "on" : "off")}";
                omitBarLine.Text = $"Skip Bar Line: {(point.NewValue?.OmitFirstBarLine == true ? "on" : "off")}";
            });
        }

        protected override EffectControlPoint CreatePoint()
        {
            var reference = Beatmap.Value.Beatmap.ControlPointInfo.EffectPointAt(SelectedGroup.Value.Time);

            return new EffectControlPoint
            {
                KiaiMode = reference.KiaiMode,
                OmitFirstBarLine = reference.OmitFirstBarLine
            };
        }
    }
}
