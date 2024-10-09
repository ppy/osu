// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class SliderCommandProxy : OsuHitObjectCommandProxy
    {
        public SliderCommandProxy(EditorCommandHandler? commandHandler, Slider hitObject)
            : base(commandHandler, hitObject)
        {
        }

        protected new Slider HitObject => (Slider)base.HitObject;

        public SliderPathCommandProxy Path => new SliderPathCommandProxy(CommandHandler, HitObject.Path);

        public double SliderVelocityMultiplier
        {
            get => HitObject.SliderVelocityMultiplier;
            set => Submit(new SetSliderVelocityMultiplierCommand(HitObject, value));
        }
    }
}
