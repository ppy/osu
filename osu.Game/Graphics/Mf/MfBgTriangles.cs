// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Graphics.Mf
{
    public class MBgTriangles : VisibilityContainer
    {
        private readonly Bindable<bool> optui = new Bindable<bool>();
        private readonly BackgroundTriangles backgroundTriangle;
        public bool EnableBeatSync { get; set; }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, OsuColour colour)
        {
            config.BindWith(MSetting.OptUI, optui);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            optui.BindValueChanged(updateIcons, true);
        }

        public MBgTriangles(float alpha = 0.65f, bool highLight = false, float triangleScale = 2f, bool sync = false)
        {
            Alpha = alpha;
            Masking = true;
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Children = new Drawable[]
            {
                backgroundTriangle = new BackgroundTriangles(highLight, triangleScale)
                {
                    BeatSync = sync,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
            };
        }

        private class BackgroundTriangles : Container
        {
            private readonly Triangles triangles;
            public readonly float TriangleScale;
            public bool BeatSync;

            public BackgroundTriangles(bool highLight = false, float triangleScaleValue = 2f)
            {
                RelativeSizeAxes = Axes.Both;
                TriangleScale = triangleScaleValue;
                Child = triangles = new Triangles
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    TriangleScale = TriangleScale,
                    Colour = highLight ? Color4Extensions.FromHex(@"88b300") : OsuColour.Gray(0.2f),
                };
            }

            protected override void LoadComplete()
            {
                triangles.EnableBeatSync = BeatSync;
            }
        }

        private void updateIcons(ValueChangedEvent<bool> value)
        {
            switch (value.NewValue)
            {
                case true:
                    backgroundTriangle.FadeIn(250);
                    break;

                case false:
                    backgroundTriangle.FadeOut(250);
                    break;
            }
        }

        protected override void PopIn()
        {
            backgroundTriangle.FadeIn(250);
        }

        protected override void PopOut()
        {
            backgroundTriangle.FadeOut(250);
        }
    }
}
