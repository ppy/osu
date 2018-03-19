using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;

namespace Symcol.Rulesets.Core.Multiplayer.Options
{
    public abstract class MultiplayerOption : Container
    {
        protected readonly SpriteText Title;

        protected readonly Container OptionContainer;

        public MultiplayerOption(string name, int quadrant, bool sync = true)
        {
            if (quadrant == 1 | quadrant == 3 | quadrant == 5 | quadrant == 7)
            {
                switch (quadrant)
                {
                    case 1:
                        quadrant = 0;
                        break;
                    case 3:
                        quadrant = 1;
                        break;
                    case 5:
                        quadrant = 2;
                        break;
                    case 7:
                        quadrant = 3;
                        break;
                }

                Anchor = Anchor.TopLeft;
                Origin = Anchor.TopLeft;
                Position = new Vector2(16, 4 + (64 * quadrant));
            }
            else if (quadrant == 2 | quadrant == 4 | quadrant == 6 | quadrant == 8)
            {
                switch (quadrant)
                {
                    case 2:
                        quadrant = 0;
                        break;
                    case 4:
                        quadrant = 1;
                        break;
                    case 6:
                        quadrant = 2;
                        break;
                    case 8:
                        quadrant = 3;
                        break;
                }

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopLeft;
                Position = new Vector2(22, 4 + (64 * quadrant));
            }
            else
                throw new Exception("Globglogabgalab");

            RelativeSizeAxes = Axes.X;
            Width = 0.49f;
            Height = 80;

            Children = new Drawable[]
            {
                Title = new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    TextSize = 20,
                    Text = name
                },
                OptionContainer = new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(-16, 18),
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }
    }
}
