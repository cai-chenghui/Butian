/**
 * Modified based on https://github.com/lolligun/ModBahuang
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
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Butian
{
    internal class AssetParam
    {
        [YamlMember(Alias = "template", Description = "该资源全局模板完整路径，如以钻木取火图片为模板则设置为 Texture/BG/zuanmuquhuo，若此资源是原版没有的，则必须设置 template", ApplyNamingConventions = false)]
        public string Template { get; set; }
        [YamlMember(Alias = "hidden",
            Description = @"隐藏原版的其他子组件
# 双修光柱就是这俩货，干掉
# hidden: [""Particle System"", ""Particle System(1)""]",
            ApplyNamingConventions = false
            )]
        public string[] Hidden { get; set; }
        [YamlMember(Alias = "assets", Description = "使用哪个资源就填上哪个，多个就填多个，必须对应 sprite 的 key，可以为空，默认取第一个", ApplyNamingConventions = false)]
        public string[] Assets { get; set; }
        [YamlMember(Alias = "sprite", Description = "图片列表，可以多个，不需要跟 assets 完全一致，可以比他多，但不能比他少哦", ApplyNamingConventions = false)]
        public Dictionary<string, SpriteParam> Sprite { get; set; }

        public static AssetParam LoadFromYaml(string path)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            try
            {
                return deserializer.Deserialize<AssetParam>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Loaded assets[{path}] failed: {e.Message}{e.GetBaseException()?.Message}");
                return null;
            }
        }
    }

    internal class SpriteParam
    {
        [YamlMember(Alias = "file", Description = "文件名，必须是同级目录下有的文件", ApplyNamingConventions = false)]
        public string File { get; set; } = "image.png";
        [YamlMember(Alias = "template", Description = "模板完整路径，跟上面的 template 可以同时设置，都设置的情况下以这个为准，如以钻木取火图片为模板则设置为 Texture/BG/zuanmuquhuo，若此资源是原版没有的，则必须设置 template", ApplyNamingConventions = false)]
        public string Template { get; set; }
        [YamlMember(Alias = "newSprite", Description = "是否是创建新的 sprite，如果为 false，则除了图片为自定义，其他均使用原版，如大小，边框，强烈建议为 false，加载速度比 true 快一倍，但原版立绘有边框，这种情况下建议用 true", ApplyNamingConventions = false)]
        public bool NewSprite { get; set; } = false;
        public Rect Rect { get; set; }
        public Vector2 Pivot { get; set; }
        [YamlMember(Alias = "pixelsPerUnit", ApplyNamingConventions = false)]
        public float PixelsPerUnit { get; set; }
        public uint Extrude { get; set; }
        [YamlMember(Alias = "meshType", ApplyNamingConventions = false)]
        public SpriteMeshType MeshType { get; set; } = SpriteMeshType.Tight;
        public Vector4 Border { get; set; }
        [YamlMember(Alias = "generateFallbackPhysicsShape", ApplyNamingConventions = false)]
        public bool GenerateFallbackPhysicsShape { get; set; } = false;
    }
    internal struct Rect
    {
        public Vector2 Position { get; set; }
        [YamlMember(Alias = "size", Description = "创建图片的大小，尽量压下图吧，图片越大越慢，还占内存", ApplyNamingConventions = false)]
        public Vector2 Size { get; set; }

        public static implicit operator UnityEngine.Rect(Rect v)
        {
            return new UnityEngine.Rect(v.Position, v.Size);
        }
    }

    internal struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static implicit operator UnityEngine.Vector2(Vector2 v)
        {
            return new UnityEngine.Vector2(v.X, v.Y);
        }
    }
    internal struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public static implicit operator UnityEngine.Vector4(Vector4 v)
        {
            return new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);
        }
    }
}
