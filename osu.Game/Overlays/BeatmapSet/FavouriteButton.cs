// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using OpenTK;

namespace osu.Game.Overlays.BeatmapSet
{
    public class FavouriteButton : HeaderButton
    {
        public readonly Bindable<bool> Favourited = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Container pink;
            SpriteIcon icon;
            AddRange(new Drawable[]
            {
                pink = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"9f015f"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = OsuColour.FromHex(@"cb2187"),
                            ColourDark = OsuColour.FromHex(@"9f015f"),
                            TriangleScale = 1.5f,
                        },
                    },
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_heart_o,
                    Size = new Vector2(18),
                    Shadow = false,
                },
            });

            Favourited.ValueChanged += value =>
            {
                if (value)
                {
                    pink.FadeIn(200);
                    icon.Icon = FontAwesome.fa_heart;
                }
                else
                {
                    pink.FadeOut(200);
                    icon.Icon = FontAwesome.fa_heart_o;
                }
            };

            Action = () => Favourited.Value = !Favourited.Value;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Width = DrawHeight;
        }
    }
}
