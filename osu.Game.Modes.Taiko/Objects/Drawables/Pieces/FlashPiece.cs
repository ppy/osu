using OpenTK;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    public class FinisherFlashPiece : FlashPiece
    {
        public FinisherFlashPiece()
        {
            Size *= 1.5f;
        }
    }

    public class FlashPiece : Container
    {
        public FlashPiece()
        {
            Size = new Vector2(128);
        }
    }
}
