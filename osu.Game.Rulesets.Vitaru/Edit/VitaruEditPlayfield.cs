using osu.Framework.Graphics.Cursor;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Vitaru.Edit.Pieces;
using osu.Game.Rulesets.Vitaru.UI;

namespace osu.Game.Rulesets.Vitaru.Edit
{
    public class VitaruEditPlayfield : VitaruPlayfield
    {
        public override bool LoadPlayer => false;

        //public override bool ProvidingUserCursor => false;

        protected override CursorContainer CreateCursor() => null;

        private readonly PatternEditor patternEditor;

        public VitaruEditPlayfield()
        {
            Add(patternEditor = new PatternEditor());
        }
    }
}
