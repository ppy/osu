using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using Symcol.Rulesets.Core.Multiplayer.Networking;
using System.Collections.Generic;

namespace Symcol.Rulesets.Core.Multiplayer.Pieces
{
    public class MultiplayerScoreboardItem : Container
    {
        public int Score
        {
            get { return score; }
            set
            {
                if (value != score)
                {
                    score = value;
                    scoreText.Text = value.ToString();

                    foreach(MultiplayerScoreboardItem item in itemList)
                        if (value > item.Score && Place > item.Place)
                        {
                            Place = item.Place;
                            foreach (MultiplayerScoreboardItem i in itemList)
                                if (i.Place < Place)
                                    i.Place -= 1;
                        }
                }
            }
        }

        public int Place
        {
            get { return place; }
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

        public readonly RulesetClientInfo ClientInfo;

        private readonly SpriteText scoreText;

        private static List<MultiplayerScoreboardItem> itemList = new List<MultiplayerScoreboardItem>();

        public MultiplayerScoreboardItem(RulesetClientInfo clientInfo, int place)
        {
            ClientInfo = clientInfo;
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
                    Text = clientInfo.Username
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
