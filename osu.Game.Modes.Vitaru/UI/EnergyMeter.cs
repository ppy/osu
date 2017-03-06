using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;
using OpenTK.Graphics;
using OpenTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.Osu.UI
{
    class EnergyMeter : PercentageCounter
    {

        private TextAwesome energyIcon;

        private Box fill;

        public EnergyMeter()
        {
            DisplayedCountSpriteText.Margin = new MarginPadding() {Right = 30, Top = 2};
            Add(new Container()
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    energyIcon = new TextAwesome()
                    {
                        Text = ((char)FontAwesome.fa_battery_empty).ToString(),
                    },
                    fill = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Margin = new MarginPadding() {Top = 5, Right = 5},
                        Colour = Color4.White,
                        Origin = Anchor.TopRight,
                        Anchor = Anchor.TopRight,
                        //Scale = new Vector2(0.75f),
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            energyIcon.TextSize = TextSize;
            fill.Height = TextSize*0.475f;
        }

        protected override string FormatCount(float count)
        {
            if(fill != null)
                fill.Width = count*0.7f;
            return base.FormatCount(count);
        }
    }
}
