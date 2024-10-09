// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Commands;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class OsuHitObjectCommandProxy : HitObjectCommandProxy
    {
        public OsuHitObjectCommandProxy(EditorCommandHandler? commandHandler, OsuHitObject hitObject)
            : base(commandHandler, hitObject)
        {
        }

        protected new OsuHitObject HitObject => (OsuHitObject)base.HitObject;

        public Vector2 Position
        {
            get => HitObject.Position;
            set => Submit(new MoveCommand(HitObject, value));
        }

        public bool NewCombo
        {
            get => HitObject.NewCombo;
            set => Submit(new SetNewComboCommand(HitObject, value));
        }
    }
}
