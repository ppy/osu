// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

            SerialisedDrawableInfo[] skinnableInfos = deserializedContent.ToArray();
            ISerialisableDrawable[] targetComponents = firstTarget.Components.ToArray();

            // Store indexes based on type for later lookup

            var targetComponentsIndexes = new Dictionary<Type, List<int>>();

            for (int i = 0; i < targetComponents.Length; i++)
            {
                Type lookup = targetComponents[i].GetType();

                if (!targetComponentsIndexes.TryGetValue(lookup, out List<int>? componentIndexes))
                    targetComponentsIndexes.Add(lookup, componentIndexes = new List<int>());

                componentIndexes.Add(i);
            }

            var indexCounting = new Dictionary<Type, int>();

            var empty = new List<int>(0);

            for (int i = 0; i < skinnableInfos.Length; i++)
            {
                Type lookup = skinnableInfos[i].Type;

                if (!targetComponentsIndexes.TryGetValue(lookup, out List<int>? componentIndexes))
                    componentIndexes = empty;

                if (!indexCounting.ContainsKey(lookup))
                    indexCounting.Add(lookup, 0);

                if (i >= componentIndexes.Count)
                    // Add new component
                    firstTarget.Add((ISerialisableDrawable)skinnableInfos[i].CreateInstance());
                else
                    // Modify existing component
                    ((Drawable)targetComponents[componentIndexes[indexCounting[lookup]++]]).ApplySerialisedInfo(skinnableInfos[i]);
            }

            foreach ((Type lookup, List<int> componentIndexes) in targetComponentsIndexes)
            {
                indexCounting.TryGetValue(lookup, out int i);

                // Remove extra components that weren't removed above
                for (; i < componentIndexes.Count; i++)
                    firstTarget.Remove(targetComponents[componentIndexes[i]], false);
            }
        }
    }
}
