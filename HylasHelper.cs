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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Butian
{
    /// <summary>
    /// 从 Hylas 导入
    /// </summary>
    public class HylasHelper
    {
        // 最大线程根据 CPU 核心数来
        private static readonly int _thread_count = Environment.ProcessorCount * 2;
        private static readonly string _hylas_home = Path.Combine(MelonUtils.GameDirectory, "Mods", "Hylas");
        private const string _param_file = "sprite.json";
        private const string _image_file = "image.png";
        private static readonly Regex _no_preview = new Regex(@"^(Battle)|(Effect)|(Texture)|(Sounds)/.*");
        private static readonly Regex _path_pattern = new Regex("^(.+/)[0-9]{3,}(/[^/]+|$)$");
        private static Dictionary<string, ManualResetEvent> _ManualEvents = new Dictionary<string, ManualResetEvent>();
        public static void Import()
        {
            if (!_hylas_home.Exists())
            {
                MelonLogger.Warning($"Hylas not found.");
                return;
            }
            // 多线程导入
            ThreadPool.SetMaxThreads(_thread_count, _thread_count);
            WaitCallback method = (t) =>
            {
                var physicalPath = t.ToString();
                ManualResetEvent manual = _ManualEvents[physicalPath];
                try
                {
                    GenerateButianAsset(physicalPath);
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"Import failed. path = {physicalPath}\nreason: {e}");
                }
                manual.Set();
            };
            foreach (var file in Directory.EnumerateFiles(_hylas_home, _param_file, SearchOption.AllDirectories))
            {
                _ManualEvents.Add(file, new ManualResetEvent(false));
                ThreadPool.QueueUserWorkItem(method, file);
            }
            foreach (var item in _ManualEvents.Values)
            {
                WaitHandle.WaitAll(new[] { item });
            }
        }

        private static void GenerateButianAsset(string physicalPath)
        {
            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                throw new ArgumentException($"“{nameof(physicalPath)}”can not be empty.", nameof(physicalPath));
            }

            var path = physicalPath.GetResourcePath(_hylas_home);
            var guiguPath = path;
            if (!_no_preview.IsMatch(path))
            {
                guiguPath = Path.Combine("Game/Portrait/", path);
            }
            var butianPath = Path.Combine(Butian.GetAssetsPath(), guiguPath);
            var overwrite = Butian.Config?.Hylas?.Overwrite == true;
            if (butianPath.Exists() && !overwrite)
            {
                MelonLogger.Warning($"Loading [{path}] has been skipped because it already exists.");
                return;
            }
            Directory.CreateDirectory(butianPath);
            File.Copy(physicalPath.Replace(_param_file, _image_file), Path.Combine(butianPath, _image_file), overwrite);

            var param = JsonConvert.DeserializeObject<SpriteParam>(File.ReadAllText(physicalPath));
            if (guiguPath.StartsWith("Game/Portrait/"))
            {
                // 立绘 都设置成新 Sprite
                param.NewSprite = true;
            }
            if (guiguPath.StartsWith("Battle/Human/"))
            {
                // 战斗小人，设置模板
                var templateId = "101";

                var match = _path_pattern.Match(path);

                var root = match.Groups[1].Value;
                var templateConfig = Path.Combine(_hylas_home, root, ".template.txt");

                if (File.Exists(templateConfig))
                {
                    templateId = File.ReadAllText(templateConfig);
                }
                param.Template = root + templateId + match.Groups[2].Value;
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithEmissionPhaseObjectGraphVisitor(args => new YamlIEnumerableSkipEmptyObjectGraphVisitor(args.InnerVisitor))
                .Build();
            var yaml = serializer.Serialize(new AssetParam
            {
                Sprite = new Dictionary<string, SpriteParam>
                    {
                        { "image", param}
                    }
            });
            File.WriteAllText(Path.Combine(butianPath, Assets.ASSET_FILE), yaml);
            if (!_no_preview.IsMatch(path))
            {
                //立绘默认设置缓存类型为缓存
                File.WriteAllText(Path.Combine(butianPath, Assets.CACHE_METHOD_FILE), "1");
            }
        }
    }
}
