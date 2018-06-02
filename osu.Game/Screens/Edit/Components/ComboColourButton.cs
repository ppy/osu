// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.UserInterface;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>
    /// A <see cref="TriangleButton"/> with customisable colours for showing a combo colour in a map.
    /// </summary>
    public class ComboColourButton : TriangleButton
    {
        private Color4 backgroundColour;
        private Color4 triangleDark;
        private Color4 triangleLight;
        public Color4 MainColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                BackgroundColour = backgroundColour;
                TriangleDark = backgroundColour.Darken(0.1f);
                TriangleLight = backgroundColour.Lighten(0.1f);
                TriggerColourChanged(value);
            }
        }
        public Color4 TriangleDark // The colours of the triangles that are already being shown do not change, maybe need to either create a new class or edit the Triangle class to update the colours of the currently shown triangles appropriately
        {
            get => triangleDark;
            set
            {
                triangleDark = value;
                if (Triangles != null)
                    Triangles.ColourDark = triangleDark;
            }
        }
        public Color4 TriangleLight
        {
            get => triangleLight;
            set
            {
                triangleLight = value;
                if (Triangles != null)
                    Triangles.ColourLight = triangleLight;
            }
        }

        /// <summary>This is raised upon changing the colour of the button.</summary>
        public event Action<Color4> ColourChanged;

        public ComboColourButton(Color4 mainColour)
        {
            MainColour = mainColour;
        }

        public void TriggerColourChanged(Color4 c)
        {
            ColourChanged?.Invoke(c);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = MainColour;
            Triangles.ColourDark = TriangleDark;
            Triangles.ColourLight = TriangleLight;
        }
    }
}
