using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Mods.Multi.Networking;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Multi.Screens.Pieces
{
    public class ScoreboardItem : Container
    {
        public int Score
        {
            get => score;
            set
            {
                if (value != score)
                {
                    score = value;
                    scoreText.Text = value.ToString();

                    foreach(ScoreboardItem item in itemList)
                        if (value > item.Score && Place > item.Place)
                        {
                            Place = item.Place;
                            foreach (ScoreboardItem i in itemList)
                                if (i.Place < Place)
                                    i.Place -= 1;
                        }
                }
            }
        }

        public int Place
        {
            get => place;
            set
            {
                if (Place != place)
                {
                    place = value;
                    this.MoveTo(new Vector2(0, (-height - 8) * (value - 1)), 200, Easing.OutQuint);
                }
            }
        }

        private int place = 0;

        private int score = 0;

        private const int height = 60;

        public readonly OsuUserInfo User;

        private readonly SpriteText scoreText;

        private static List<ScoreboardItem> itemList = new List<ScoreboardItem>();

        public ScoreboardItem(OsuUserInfo user, int place)
        {
            User = user;
            this.place = place;

            itemList.Add(this);

            RelativeSizeAxes = Axes.X;
            Height = height;

            Masking = true;
            CornerRadius = 8;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                },
                new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(4),
                    Text = User.Username
                },
                scoreText = new SpriteText
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(-4),
                    Text = Score.ToString()
                }
            };

            this.MoveTo(new Vector2(0, (-height - 8) * (Place - 1)), 200, Easing.OutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            itemList.Remove(this);
            base.Dispose(isDisposing);
        }
    }
}
