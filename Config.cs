/**
 * Copyright 2021 墨涤千尘（LangYueMC）
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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Butian
{
    internal class Config
    {
        private static float CurrentVersion = 1.0F;
        public float Version { get; set; } = CurrentVersion;
        public PrefetchFilter Prefetch { get; set; }

        public static Config Default() => new Config
        {
            Prefetch = new PrefetchFilter()
        };

        public static Config LoadConfig()
        {
            Config config = null;
            var configPath = Path.Combine(Butian.GetHome(), "config.yml");
            if (configPath.Exists())
            {
                MelonLogger.Msg("Loading configuration...");
                var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                try
                {
                    config = deserializer.Deserialize<Config>(File.ReadAllText(configPath));
                    MelonLogger.Msg("Loaded successfully");
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"Load configuration failed, will use the default: {e.Message}{e.GetBaseException()?.Message}");
                    File.Copy(configPath, String.Format("{0}{1}", configPath, ".back"), true);
                }
            }
            var needSave = false;
            if (config == null)
            {
                config = Config.Default();
                MelonLogger.Msg($"Saving default configuration...");
                needSave = true;
            }
            else if (config.Version < CurrentVersion)
            {
                config.Version = CurrentVersion;
                MelonLogger.Msg($"Saving configuration v{config.Version}...");
                needSave = true;
            }
            if (needSave)
            {
                var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                var yaml = serializer.Serialize(config);
                File.WriteAllText(configPath, yaml);
                MelonLogger.Msg("Saved successfully");
            }
            return config;
        }
    }
    internal class PrefetchFilter
    {
        public bool Enable { get; set; } = false;
        [YamlMember(Alias = "whitelist", ApplyNamingConventions = false)]
        public string[] WhitelistStr { get; set; } = new string[] { "^Game/Portrait/.*$" };
        [YamlIgnore]
        public List<Regex> Whitelist => WhitelistStr.Select(x => new Regex(x)).ToList();
    }
}
