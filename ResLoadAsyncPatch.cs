/**
 * Code from https://github.com/lolligun/ModBahuang
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
using System;
using Object = UnityEngine.Object;

namespace Butian
{
    [HarmonyPatch(typeof(ResMgr), "LoadAsync")]
    public class ResLoadAsyncPatch
    {
        public static bool Prefix(ref string path, Action<Object> call)
        {
            MelonDebug.Msg($"ResMgr::LoadAsync({path})");

            // var exist = Cache.TryGet(path, out var go);
            var exist = Assets.TryGet(path, out var __result);

            if (!exist) return true;

            // It's a workaround for `call` that used in `Wrapper` got freed in Il2cpp domain
            // I have no idea why/how this could work. But, though, anyway, it just WORKS.
            // A reason might be the `native` will keep a gc handle to prevent `call` from freeing.
            Il2CppSystem.Action<Object> native = call;

            native.Invoke(__result);

            return false;
        }
    }
}
