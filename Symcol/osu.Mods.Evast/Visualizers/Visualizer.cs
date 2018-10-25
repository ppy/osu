// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Core.Screens.Evast;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Mods.Evast.MusicVisualizers;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Evast.Visualizers
{
    public class Visualizer : BeatmapScreen
    {
        protected override bool HideOverlaysOnEnter => true;

        private BeatmapSprite beatmapSprite;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SpaceParticlesContainer(),
                new CircularVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 180,
                    BarsAmount = 100,
                    CircleSize = 348,
                    BarWidth = 2,
                },
                new CircularVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 180,
                    BarsAmount = 100,
                    CircleSize = 348,
                    BarWidth = 2,
                    Rotation = 180,
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(350),
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.18f),
                        Offset = new Vector2(0, 2),
                        Radius = 6,
                    },
                    Child = beatmapSprite = new BeatmapSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fill,
                    }
                },
            };
        }

        protected override void OnBeatmapChange(WorkingBeatmap beatmap)
        {
            base.OnBeatmapChange(beatmap);
            beatmapSprite.UpdateTexture(beatmap);
        }

        private class BeatmapSprite : Sprite
        {
            public void UpdateTexture(WorkingBeatmap beatmap)
            {
                if (beatmap.Background != null)
                    Texture = beatmap.Background;
            }
        }
    }
}
