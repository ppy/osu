// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Graphics
{
    public class MfBgTriangles : Container
    {
        public const float HEIGHT = 50;
        private readonly Bindable<bool> Optui = new Bindable<bool>();
        private BackgroundTriangles BackgroundTriangle;
        
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuColour colour)
        {
            config.BindWith(OsuSetting.OptUI, Optui);

            Optui.ValueChanged += _ => UpdateIcons();
            UpdateIcons();
        }

        public MfBgTriangles(float height = HEIGHT, float alpha = 0.65f, bool highLight = false)
        {
            this.Height = height;
            this.Alpha = alpha;
            RelativeSizeAxes = Axes.X;
            Height = height;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Children = new Drawable[]
            {
                BackgroundTriangle = new BackgroundTriangles(height, highLight)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
            };
        }

        public class BackgroundTriangles : Container
        {
            private readonly Triangles triangles;

            public BackgroundTriangles(float height = HEIGHT, bool highLight = false)
            {
                RelativeSizeAxes = Axes.X;
                Height = height;
                Masking = true;
                Children = new Drawable[]
                {
                    triangles = new Triangles
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.Both,
                        TriangleScale = 2,
                        Colour = highLight ? Color4Extensions.FromHex(@"88b300") : OsuColour.Gray(0.2f),
                    },
                };
            }
        }

        private void UpdateIcons()
        {
            switch (Optui.Value)
            {
                case true:
                    BackgroundTriangle.FadeIn(250);
                    break;

                case false:
                    BackgroundTriangle.FadeOut(250);
                    break;
            }

        }

    }
}