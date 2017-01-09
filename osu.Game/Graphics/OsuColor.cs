using System;
using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Graphics
{
    public static class OsuColor
    {
        public static readonly Color4 OsuPink = new Color4(255, 102, 170, 255);
    
        public static readonly Color4 BeatmapPanelUnselected = new Color4(20, 43, 51, 255);
        public static readonly ColourInfo BeatmapPanelSelected = ColourInfo.GradientVertical(
                new Color4(20, 43, 51, 255),
                new Color4(40, 86, 102, 255));
                
        public static readonly ColourInfo BeatmapHeaderBackgroundA =
            ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.9f));
        public static readonly ColourInfo BeatmapHeaderBackgroundB =
            ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f));
        public static readonly ColourInfo BeatmapHeaderBackgroundC =
            ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0));

        public static readonly Color4 PanelBorder = new Color4(221, 255, 255, 255);
        public static readonly Color4 PanelGlowSelected = new Color4(130, 204, 255, 150);
        public static readonly Color4 PanelGlowUnselected = new Color4(0, 0, 0, 100);

        public static readonly Color4 Combo1 = new Color4(17, 136, 170, 255);
        public static readonly Color4 Combo2 = new Color4(102, 136, 0, 255);
        public static readonly Color4 Combo3 = new Color4(204, 102, 0, 255);
        public static readonly Color4 Combo4 = new Color4(121, 9, 13, 255);

        public static readonly Color4 BackButtonLeft = new Color4(195, 40, 140, 255);
        public static readonly Color4 BackButtonRight = new Color4(238, 51, 153, 255);

        public static readonly Color4 Button = new Color4(14, 132, 165, 255);

        public static readonly Color4 PlayButton = new Color4(238, 51, 153, 255);

        public static readonly Color4 MusicControllerBackground = new Color4(150, 150, 150, 255);
        public static readonly Color4 MusicControllerProgress = new Color4(255, 204, 34, 255);

        public static readonly Color4 CheckBoxHover = new Color4(255, 221, 238, 255);
        public static readonly Color4 CheckBoxGlow = new Color4(187, 17, 119, 0);

        public static readonly Color4 DropDownBackground = new Color4(0, 0, 0, 128);
        public static readonly Color4 DropDownHover = new Color4(187, 17, 119, 255);

        public static readonly Color4 OptionSectionHeader = new Color4(247, 198, 35, 255);

        public static readonly Color4 SidebarButtonBackground = new Color4(60, 60, 60, 255);
        public static readonly Color4 SidebarButtonSelectionIndicator = new Color4(247, 198, 35, 255);

        public static readonly Color4 SliderbarBackground = new Color4(255, 102, 170, 255);
        public static readonly Color4 SliderbarNub = new Color4(255, 102, 170, 255);

        public static readonly Color4 ToolbarModeButtonIcon = new Color4(255, 194, 224, 255);
        public static readonly Color4 ToolbarModeButtonIconActiveGlow = new Color4(255, 194, 224, 100);

        public static readonly Color4 BeatmapInfoWedgeBorder = new Color4(221, 255, 255, 255);
        public static readonly Color4 BeatmapInfoWedgeGlow = new Color4(130, 204, 255, 150);
    }
}