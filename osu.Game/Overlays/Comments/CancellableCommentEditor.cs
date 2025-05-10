// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public abstract partial class CancellableCommentEditor : CommentEditor
    {
        public Action? OnCancel;

        [BackgroundDependencyLoader]
        private void load()
        {
            ButtonsContainer.Add(new EditorButton
            {
                Action = () => OnCancel?.Invoke(),
                Text = CommonStrings.ButtonsCancel,
            });
        }
    }
}
