using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Beatmaps.ControlPoints;
using System;
using osu.Framework.Configuration;
using osu.Framework.Audio.Track;
using System.Collections.Generic;
using OpenTK.Input;

namespace osu.Game.Screens.Symcol.Pieces
{
    public class GeneralButton : Sprite
    {
        public Action Action;
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => this.ReceiveMouseInputAt(screenSpacePos);

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Action();
            return true;
        }
    }
}
