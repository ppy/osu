using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Osu.UI
{
    class EnergyMeter : PercentageCounter
    {

        private TextAwesome energyIcon;

        public EnergyMeter()
        {
            DisplayedCountSpriteText.Margin = new MarginPadding() {Right = 30, Top = 2};
            Add(energyIcon = new TextAwesome()
            {
                Text = ((char)FontAwesome.fa_battery).ToString(),
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            energyIcon.TextSize = TextSize;
        }

        protected override string FormatCount(float count)
        {
            FontAwesome newIcon = FontAwesome.fa_battery_empty;
            switch ((int)(count / 0.25f))
            {
                case 4:
                    newIcon = FontAwesome.fa_battery_full;
                    break;
                case 3:
                    newIcon = FontAwesome.fa_battery_three_quarters;
                    break;
                case 2:
                    newIcon = FontAwesome.fa_battery_half;
                    break;
                case 1:
                    newIcon = FontAwesome.fa_battery_quarter;
                    break;
            }
            if(energyIcon != null)
                energyIcon.Text = ((char)newIcon).ToString();
            return base.FormatCount(count);
        }

        protected override void TransformCount(float currentValue, float newValue)
        {
            base.TransformCount(currentValue, newValue);
            
        }
    }
}
