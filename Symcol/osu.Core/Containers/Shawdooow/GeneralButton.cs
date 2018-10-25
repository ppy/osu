using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using OpenTK;

namespace osu.Core.Containers.Shawdooow
{
    public class GeneralButton : Sprite
    {
        public Action Action;
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => this.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Action();
            return true;
        }
    }
}
