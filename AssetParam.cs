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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Butian
{
    internal class AssetParam
    {
        public string[] Hidden { get; set; }
        public string[] Assets { get; set; }
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
        public string File { get; set; } = "image.png";
        public string Template { get; set; }
        [YamlMember(Alias = "newSprite", ApplyNamingConventions = false)]
        public bool NewSprite { get; set; } = false;
        public Rect Rect { get; set; }
        public Vector2 Pivot { get; set; }
        [YamlMember(Alias = "pixelsPerUnit", ApplyNamingConventions = false)]
        public float PixelsPerUnit { get; set; }
        public uint Extrude { get; set; }
        [YamlMember(Alias = "meshType", ApplyNamingConventions = false)]
        public SpriteMeshType MeshType { get; set; }
        public Vector4 Border { get; set; }
        [YamlMember(Alias = "generateFallbackPhysicsShape", ApplyNamingConventions = false)]
        public bool GenerateFallbackPhysicsShape { get; set; }
    }
    internal struct Rect
    {
        public Vector2 Position { get; set; }
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
