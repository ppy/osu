// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Direct
{
    public abstract class DirectPanel : Container
    {
        protected virtual Sprite Background { get; }
        protected virtual OsuSpriteText Title { get; }
        protected virtual OsuSpriteText Artist { get; }
        protected virtual OsuSpriteText Mapper { get; }
        protected virtual OsuSpriteText Source { get; }
        protected virtual Statistic PlayCount { get; }
        protected virtual Statistic FavouriteCount { get; }
        protected virtual FillFlowContainer DifficultyIcons { get; }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Background.Texture = textures.Get(@"Backgrounds/bg4");

            Title.Text = @"Platina";
            Artist.Text = @"Maaya Sakamoto";
            Mapper.Text = @"TicClick";
            Source.Text = @"from Cardcaptor Sakura";
            PlayCount.Value = 4579492;
            FavouriteCount.Value = 2659;
        }

        public class Statistic : FillFlowContainer
        {
            private readonly SpriteText text;

            private int value;
            public int Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    text.Text = string.Format("{0:n0}", Value);
                }
            }

            public Statistic(FontAwesome icon, int value = 0)
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5f, 0f);

                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Font = @"Exo2.0-SemiBoldItalic",
                    },
                    new TextAwesome
                    {
                        Icon = icon,
                        Shadow = true,
                        TextSize = 14,
                        Margin = new MarginPadding { Top = 1 },
                    },
                };

                Value = value;
            }
        }
    }
}
