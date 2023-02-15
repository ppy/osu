// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinEditorChangeHandler : EditorChangeHandler
    {
        private readonly ISerialisableDrawableContainer? firstTarget;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BindableList<ISerialisableDrawable>? components;

        public SkinEditorChangeHandler(Drawable targetScreen)
        {
            // To keep things simple, we are currently only handling the current target screen for undo / redo.
            // In the future we'll want this to cover all changes, even to skin's `InstantiationInfo`.
            // We'll also need to consider cases where multiple targets are on screen at the same time.

            firstTarget = targetScreen.ChildrenOfType<ISerialisableDrawableContainer>().FirstOrDefault();

            if (firstTarget == null)
                return;

            components = new BindableList<ISerialisableDrawable> { BindTarget = firstTarget.Components };
            components.BindCollectionChanged((_, _) => SaveState());
        }

        protected override void WriteCurrentStateToStream(MemoryStream stream)
        {
            if (firstTarget == null)
                return;

            var skinnableInfos = firstTarget.CreateSerialisedInfo().ToArray();
            string json = JsonConvert.SerializeObject(skinnableInfos, new JsonSerializerSettings { Formatting = Formatting.Indented });
            stream.Write(Encoding.UTF8.GetBytes(json));
        }

        protected override void ApplyStateChange(byte[] previousState, byte[] newState)
        {
            if (firstTarget == null)
                return;

            var deserializedContent = JsonConvert.DeserializeObject<IEnumerable<SerialisedDrawableInfo>>(Encoding.UTF8.GetString(newState));

            if (deserializedContent == null)
                return;

            SerialisedDrawableInfo[] skinnableInfo = deserializedContent.ToArray();
            Drawable[] targetComponents = firstTarget.Components.OfType<Drawable>().ToArray();

            if (!skinnableInfo.Select(s => s.Type).SequenceEqual(targetComponents.Select(d => d.GetType())))
            {
                // Perform a naive full reload for now.
                firstTarget.Reload(skinnableInfo);
            }
            else
            {
                int i = 0;

                foreach (var drawable in targetComponents)
                    drawable.ApplySerialisedInfo(skinnableInfo[i++]);
            }
        }
    }
}
