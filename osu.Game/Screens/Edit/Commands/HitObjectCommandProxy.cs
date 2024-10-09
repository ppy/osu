// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public class HitObjectCommandProxy : CommandProxy
    {
        public HitObjectCommandProxy(EditorCommandHandler? commandHandler, HitObject hitObject)
            : base(commandHandler)
        {
            HitObject = hitObject;
        }

        protected HitObject HitObject;

        public double StartTime
        {
            get => HitObject.StartTime;
            set => Submit(new SetStartTimeCommand(HitObject, value));
        }
    }
}
