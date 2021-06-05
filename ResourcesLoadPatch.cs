/**
 * Code from  https://github.com/lolligun/ModBahuang
 * 
 * Copyright 2021 lolligun
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Harmony;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Butian
{
    class ResourcesLoadPatch
    {

        public static void Patch(HarmonyInstance harmony)
        {
            var original = typeof(Resources).GetMethod("Load", new[] { typeof(string), typeof(Il2CppSystem.Type) });
            harmony.Patch(original, prefix: new HarmonyMethod(typeof(ResourcesLoadPatch).GetMethod(nameof(Prefix))));
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once RedundantAssignment
        public static bool Prefix(ref Object __result, string path, Il2CppSystem.Type systemTypeInstance)
        {
            MelonDebug.Msg($"Resources::Load({path}, {systemTypeInstance.Name})");
            var exist = Assets.TryGet(path, out __result);

            return !exist;
        }
    }
}
