﻿using RoR2;
using RoR2.Audio;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// Utility methods used by MoonstormSharedUtils
    /// </summary>
    public static class MSUtil
    {
        private static Run currentRun;
        private static ExpansionDef[] currentRunExpansionDefs;
        /// <summary>
        /// Checks if a mod is installed in the bepinex chainloader
        /// </summary>
        /// <param name="GUID">The GUID of the mod to check.</param>
        /// <returns>True if installed, false otherwise.</returns>
        public static bool IsModInstalled(string GUID)
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);
        }


        /// <summary>
        /// Calculates inverse hyperbolic scaling (diminishing) for the parameters passed in, and returns the result.
        /// <para>Uses the formula: baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)))</para>
        /// <para>Original code by KomradeSpectre</para>
        /// </summary>
        /// <param name="baseValue">The starting value of the function.</param>
        /// <param name="additionalValue">The value that is added per additional itemCount</param>
        /// <param name="maxValue">The maximum value that the function can possibly be.</param>
        /// <param name="itemCount">The amount of items/stacks that increments our function.</param>
        /// <returns>A float representing the inverse hyperbolic scaling of the parameters.</returns>
        public static float InverseHyperbolicScaling(float baseValue, float additionalValue, float maxValue, int itemCount)
        {
            return baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)));
        }

        /// <summary>
        /// Shorthand for playing a networked sound event def
        /// </summary>
        /// <param name="soundEventName">The name of the sound event</param>
        /// <param name="pos">Position at wich to play the sound</param>
        /// <param name="transmit"></param>
        public static void PlayNetworkedSFX(string soundEventName, Vector3 pos, bool transmit = true)
        {
            var soundID = NetworkSoundEventCatalog.FindNetworkSoundEventIndex(soundEventName);
            if (soundID == NetworkSoundEventIndex.Invalid)
            {
                MSULog.Warning($"Could not find sound event with name of {soundEventName}");
                return;
            }
            EffectManager.SimpleSoundEffect(soundID, pos, transmit);
        }

        /// <summary>
        /// Returns all the enabled expansions for the current run
        /// </summary>
        /// <param name="run">The current run</param>
        /// <returns>An array of expansionDefs</returns>
        public static ExpansionDef[] GetEnabledExpansions(this Run run)
        {
            if (currentRun == run)
            {
                return currentRunExpansionDefs;
            }

            currentRun = run;
            currentRunExpansionDefs = ExpansionCatalog.expansionDefs.Where(x => run.IsExpansionEnabled(x)).ToArray();
            return currentRunExpansionDefs;
        }

        #region Extensions
        /// <summary>
        /// Plays the network sound event def.
        /// </summary>
        /// <param name="pos">The position where the sound will play</param>
        /// <param name="transmit"></param>
        public static void Play(this NetworkSoundEventDef eventDef, Vector3 pos, bool transmit = true)
        {
            if (eventDef.index == NetworkSoundEventIndex.Invalid)
            {
                MSULog.Warning($"{eventDef} has an invalid network sound event index.");
                return;
            }
            EffectManager.SimpleSoundEffect(eventDef.index, pos, transmit);
        }

        /// <summary>
        /// Ensures that the component specified in <typeparamref name="T"/> exists
        /// Basically Gets the component, if it doesnt exist, it adds it then returns it.
        /// </summary>
        /// <typeparam name="T">The type of component to ensure</typeparam>
        /// <returns>The component <typeparamref name="T"/></returns>
        public static T EnsureComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            var comp = obj.GetComponent<T>();
            if (!comp)
                comp = obj.AddComponent<T>();

            return comp;
        }

        /// <summary>
        /// Adds the entry of type <typeparamref name="T"/> into the collection if its not already in it
        /// </summary>
        /// <typeparam name="T">The type of item in the collection</typeparam>
        /// <param name="collection"></param>
        /// <param name="entry">The entry to add if its not in the collection</param>
        /// <returns>True if it was not in the collection and added, false otherwise</returns>
        public static bool AddIfNotInCollection<T>(this ICollection<T> collection, T entry)
        {
            if (collection.Contains(entry))
                return false;
            collection.Add(entry);
            return true;
        }

        [Obsolete("Method is wrongly named, use RemoveIfInCollection instead")]
        public static bool RemoveIfNotInCollection<T>(this ICollection<T> collection, T entry)
        {
            return RemoveIfInCollection(collection, entry);
        }

        /// <summary>
        /// Removes the entry of type <typeparamref name="T"/> from the collection if its in it
        /// </summary>
        /// <typeparam name="T">The type of item in the collection</typeparam>
        /// <param name="collection"></param>
        /// <param name="entry">The entry to remove if its in the collection</param>
        /// <returns>True if it was in the collection and removed, false otherwise</returns>
        public static bool RemoveIfInCollection<T>(this ICollection<T> collection, T entry)
        {
            if (!collection.Contains(entry))
                return false;
            collection.Remove(entry);
            return true;
        }

        /// <summary>
        /// Returns the amount of stacks of <paramref name="itemDef"/> that the body has.
        /// </summary>
        /// <param name="itemDef">The ItemDef to count</param>
        /// <returns>The amount of items, returns 0 if the body doesnt have an inventory.</returns>
        public static int GetItemCount(this CharacterBody body, ItemDef itemDef)
        {
            return body.inventory == null ? 0 : body.inventory.GetItemCount(itemDef);
        }

        /// <summary>
        /// Returns the amount of stacks of <paramref name="index"/> that the body has.
        /// </summary>
        /// <param name="index">The ItemIndex to count</param>
        /// <returns>The amount of items, returns 0 if the body doesnt have an inventory.</returns>
        public static int GetItemCount(this CharacterBody body, ItemIndex index)
        {
            return body.inventory == null ? 0 : body.inventory.GetItemCount(index);
        }

        /// <summary>
        /// Gets all the types from an assembly safely by getting the types from a ReflectionTypeLoadException if one is thrown
        /// </summary>
        /// <returns>The types of the assembly</returns>
        public static Type[] GetTypesSafe(this Assembly assembly)
        {
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException re)
            {
                types = re.Types.Where(t => t != null).ToArray();
            }
            return types;
        }

        /// <summary>
        /// Deconstruct a KeyValuePair into two variables, useful for iterating over a dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of Key of the KeyValuePair</typeparam>
        /// <typeparam name="TValue">The type of Value of the KeyValuePair</typeparam>
        /// <param name="kvp">The KeyValuePair to deconstruct</param>
        /// <param name="key">The Key of the KeyValuePair</param>
        /// <param name="value">The Value of the KeyValuePair</param>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        /// <summary>
        /// Nicifies a CamelCase or pascalCase string into a human readable name
        /// </summary>
        /// <param name="text">The string to nicify</param>
        /// <returns>The nicified string</returns>
        public static string NicifyString(string text)
        {
            string origName = new string(text.ToCharArray());
            try
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                List<char> nameAsChar = null;
                if (text.StartsWith("m_", System.StringComparison.OrdinalIgnoreCase) || text.StartsWith("k_", System.StringComparison.OrdinalIgnoreCase))
                {
                    nameAsChar = text.Substring("m_".Length).ToList();
                }
                else
                {
                    nameAsChar = text.ToList();
                }

                while (nameAsChar.First() == '_')
                {
                    nameAsChar.RemoveAt(0);
                }
                List<char> newText = new List<char>();
                for (int i = 0; i < nameAsChar.Count; i++)
                {
                    char character = nameAsChar[i];
                    if (i == 0)
                    {
                        if (char.IsLower(character))
                        {
                            newText.Add(char.ToUpper(character));
                        }
                        else
                        {
                            newText.Add(character);
                        }
                        continue;
                    }

                    if (char.IsUpper(character))
                    {
                        newText.Add(' ');
                        newText.Add(character);
                        continue;
                    }
                    newText.Add(character);
                }
                return new String(newText.ToArray());
            }
            catch (Exception e)
            {
                MSULog.Error($"Failed to nicify {origName}: {e}");
                return origName;
            }
        }

        /// <summary>
        /// Returns the weighted selection's choices' values.
        /// </summary>
        /// <typeparam name="T">The type that's used in the WeightedSelection</typeparam>
        /// <param name="selection">The weighted selectiton instance</param>
        /// <returns>An IEnumerable will all the weighted selectiton's values that are not null.</returns>
        public static IEnumerable<T> GetValues<T>(this WeightedSelection<T> selection)
        {
            return selection.choices.Select(x => x.value).Where(x => x != null);
        }

        public static T AsValidOrNull<T>(this T obj) where T : UnityEngine.Object
        {
            return obj ? obj : null;
        }
        #endregion
    }
}
