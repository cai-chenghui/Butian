/**
 * Copyright 2021 墨涤千尘（LangYueMc）
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
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Butian
{
    internal class Bridger
    {
        private static readonly GameObject _go_template = new GameObject
        {
            name = "template",
            active = false
        };
        private static Dictionary<Regex, string> _assetes_type = new Dictionary<Regex, string>();
        private static readonly string _assetes_type_map_path = Path.Combine(Butian.GetHome(), "_assetes_type_map");


        static Bridger()
        {
            var renderer = _go_template.AddComponent(Il2CppType.Of<SpriteRenderer>()).Cast<SpriteRenderer>();
            Object.DontDestroyOnLoad(_go_template);
            if (!_assetes_type_map_path.Exists())
            {
                // 后续联网下载
                File.WriteAllLines(_assetes_type_map_path, new string[] {
                    "^Game/Portrait/.*$§GameObject",
                    "^Battle/Human/.*$§GameObject",
                    "^Effect/UI/.*$§GameObject",
                    "^Texture/.*$§Sprite",
                    "^Sounds/.*$§AudioClip",
                });
            }
            foreach (var line in File.ReadAllLines(_assetes_type_map_path))
            {
                var l = line.Split('§');
                _assetes_type.Add(new Regex(l[0]), l[1]);
            }

        }
        public static Il2CppSystem.Type GetIl2CppType(string path)
        {
            foreach (var item in _assetes_type)
            {
                if (item.Key.IsMatch(path))
                {
                    switch (item.Value)
                    {
                        case "Object":
                        case "GameObject":
                            return Il2CppType.Of<GameObject>();
                        case "Sprite":
                            return Il2CppType.Of<Sprite>();
                        case "AudioClip":
                            return Il2CppType.Of<AudioClip>();
                        default:
                            return Il2CppType.Of<Object>();
                    }
                }
            }
            return null;
        }

        public static Object Bridge(string path, Il2CppSystem.Type il2CppType = null)
        {
            if (il2CppType == null)
            {
                il2CppType = GetIl2CppType(path);
            }
            if (il2CppType == null)
            {
                throw new NotSupportedException($"Unsupported type for {path}");
            }
            var param = AssetParam.LoadFromYaml(Path.Combine(AbsolutelyPhysicalPath(path), Assets.ASSET_FILE));
            var t = path.ResourcesLoad(il2CppType);
            switch (il2CppType.Name)
            {
                case "Object":
                case "GameObject":
                    return BridgeImage(path, param, t?.TryCast<GameObject>());
                case "Sprite":
                    var imgGo = Object.Instantiate(_go_template);
                    imgGo.name = t?.name ?? path.Replace("/", "");
                    var go = BridgeImage(path, param, imgGo, true);
                    var renderer = go.GetComponentInChildren<SpriteRenderer>();
                    return renderer.sprite;
                default:
                    MelonLogger.Warning($"Type[{il2CppType.Name}] not currently supported");
                    return t;
            }

        }

        private static GameObject BridgeImage(string path, AssetParam param, GameObject imageObj, bool isSprite = false)
        {
            MelonDebug.Msg($"BridgeImage {path} 1");
            var assets = param.Assets;
            var spriteParam = param.Sprite[assets?.Length > 0 ? assets[0] : param.Sprite?.Keys?.First()];
            if (spriteParam == null)
            {
                throw new NullReferenceException("SpriteParam is null");
            }
            MelonDebug.Msg($"BridgeImage {path} 2");
            var templatePath = spriteParam.Template ?? param.Template;
            GameObject template = null;
            if (templatePath != null)
            {
                var temp = templatePath.ResourcesLoad();
                if (temp != null)
                    if (isSprite)
                    {
                        var sRenderer = imageObj.GetComponentInChildren<SpriteRenderer>();
                        sRenderer.sprite = Object.Instantiate(temp.Cast<Sprite>());
                        template = imageObj;
                    }
                    else
                    {
                        template = Object.Instantiate(temp.Cast<GameObject>());
                    }
            }
            MelonDebug.Msg($"BridgeImage {path} 3");
            var ic = Assets.CopyAsset(path, template ?? imageObj);
            if (ic == null)
            {
                throw new NullReferenceException("GameObject copy is null");
            }
            MelonDebug.Msg($"BridgeImage {path} 4");
            if (param.Hidden?.Length > 0)
            {
                var nodes = ic.GetComponentsInChildren<Transform>().Where(i => param.Hidden.Contains(i.name));
                foreach (Transform child in nodes)
                {
                    child.gameObject.SetActive(false);
                }
            }
            MelonDebug.Msg($"BridgeImage {path} 5");
            var image = File.ReadAllBytes(Path.Combine(AbsolutelyPhysicalPath(path), spriteParam?.File));
            if (image == null || image.Length == 0)
            {
                throw new NullReferenceException($"File {spriteParam?.File} is null");
            }
            MelonDebug.Msg($"BridgeImage {path} 6");
            var renderer = ic.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                throw new NullReferenceException("SpriteRenderer is null");
            }
            MelonDebug.Msg($"BridgeImage {path} 7");
            if (spriteParam.NewSprite || renderer.sprite == null)
            {
                if (Math.Min(spriteParam.Rect.Size.X, spriteParam.Rect.Size.Y) > 1024)
                {
                    MelonLogger.Warning($"The picture[{path}] is too big, Compress it!!! pleeeease~");
                }
                Texture2D texture = new Texture2D((int)spriteParam.Rect.Size.X, (int)spriteParam.Rect.Size.Y, TextureFormat.ARGB32, false);
                if (!ImageConversion.LoadImage(texture, image))
                {
                    throw new InvalidOperationException("Image failed to load");
                }
                renderer.sprite = Sprite.Create(texture, spriteParam.Rect, spriteParam.Pivot, spriteParam.PixelsPerUnit, spriteParam.Extrude, spriteParam.MeshType, spriteParam.Border, spriteParam.GenerateFallbackPhysicsShape);
            }
            else
            {
                if (!ImageConversion.LoadImage(renderer.sprite.texture, image))
                {
                    throw new InvalidOperationException("Image failed to load");
                }
            }
            MelonDebug.Msg($"BridgeImage {path} 8");
            return ic;
        }

        private static string AbsolutelyPhysicalPath(string path) => Path.GetFullPath(Path.Combine(Butian.GetAssetsPath(), path));
    }
}
