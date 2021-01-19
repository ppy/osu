using osu.Game.Screens.Purcashe.Components;
using osuTK.Graphics;

namespace osu.Game.Screens.Purcashe
{
    public static class PurcasheColorProvider
    {
        public static Color4 GetColor(ItemPanel.Rank rnk)
        {
            switch (rnk)
            {
                case ItemPanel.Rank.Oops:
                    return Color4.DarkRed;

                case ItemPanel.Rank.Common:
                    return Color4.LightGreen;

                case ItemPanel.Rank.Rare:
                    return Color4.Silver;

                case ItemPanel.Rank.Legendary:
                    return Color4.Gold;

                default:
                    return Color4.Gray;
            }
        }
    }
}
