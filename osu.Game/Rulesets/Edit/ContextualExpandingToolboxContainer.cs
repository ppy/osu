// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// An <see cref="ExpandingToolboxContainer"/> that allows temporarily displaying contextual toolbox groups as ít's first item
    /// </summary>
    public partial class ContextualExpandingToolboxContainer : ExpandingToolboxContainer
    {
        public ContextualExpandingToolboxContainer(float contractedWidth, float expandedWidth)
            : base(contractedWidth, expandedWidth)
        {
        }

        private EditorToolboxGroup? contextualToolboxGroup;

        public EditorToolboxGroup? ContextualToolboxGroup
        {
            get => contextualToolboxGroup;
            set
            {
                if (contextualToolboxGroup == value)
                    return;

                if (contextualToolboxGroup != null)
                    FillFlow.Remove(contextualToolboxGroup, true);

                contextualToolboxGroup = value;

                if (value != null)
                    FillFlow.Insert(-1, value);
            }
        }
    }
}
