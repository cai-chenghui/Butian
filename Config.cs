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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Butian
{
    internal class Config
    {
        [YamlIgnore]
        private static readonly string configPath = Path.Combine(Butian.GetHome(), "config.yml");
        [YamlIgnore]
        private const int CurrentVersion = 2;

        [YamlMember(Alias = "version", Description = "配置文件版本，跟补天版本不同步，用来自动更新配置文件的，不建议修改（可以修改更小的版本让补天自动获取最新版本配置）", ApplyNamingConventions = false)]
        public int Version { get; set; } = CurrentVersion;
        [YamlMember(Alias = "prefetch",
            Description = @"预加载配置
# 其实除了立绘，其他的完全没必要开启预加载，就算立绘也可以不开启，然后设置为缓存，这样就只有第一次进城镇可能会慢点
# 开启预加载是在游戏开始界面就开始加载资源，跟缓存用的时间一样，但是缓存等于是分散了时间
# 建议不开启预加载，然后在单个资源目录，也就是 asset.yml 同级目录下放个文件 .cache.method
# .cache.method 文件内容
# 0 代表这个资源每次都从磁盘读取（没有这个文件并且不在预加载白名单的默认就是0，所以可以直接不放这个文件）
# 1 代表会缓存，只有第一次会从磁盘读取（立绘之类的访问频率高的建议设置）
# 2 预加载，从游戏主界面就预加载并放进缓存（若未开启预加载，会自动设置为缓存）",
            ApplyNamingConventions = false)]
        public PrefetchFilter Prefetch { get; set; }
        public Hylas Hylas { get; set; }

        public static Config Default() => new Config
        {
            Prefetch = new PrefetchFilter(),
            Hylas = new Hylas()
        };

        public static Config LoadConfig()
        {
            Config config = null;
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
                // 若没有配置文件，默认认为是新用户，默认导入
                config.Hylas.Import = true;
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
                config.Save();
            }
            return config;
        }

        internal void Save()
        {
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yaml = serializer.Serialize(this);
            File.WriteAllText(configPath, yaml);
            MelonLogger.Msg("Configuration file saved successfully");
        }
    }
    internal class PrefetchFilter
    {
        [YamlMember(Alias = "enable", Description = "是否开启预加载，若为false，则不开启，并且单个资源 .cache.method 文件配置的预加载也不生效，只能缓存", ApplyNamingConventions = false)]
        public bool Enable { get; set; } = false;
        [YamlMember(Alias = "whitelist", Description = "预加载白名单，不在白名单列表中的不会预加载，正则方式，可以自行扩展，最终以 .cache.method 文件配置为准，.cache.method可以不放", ApplyNamingConventions = false)]
        public string[] WhitelistStr { get; set; } = new string[] { "^Game/Portrait/.*$" };
        [YamlIgnore]
        public List<Regex> Whitelist => WhitelistStr.Select(x => new Regex(x)).ToList();
    }
    internal class Hylas
    {
        [YamlMember(Alias = "import", Description = "是否开启从 Hylas 导入，开启后下次启动游戏补天会自动导入", ApplyNamingConventions = false)]
        public bool Import { get; set; } = false;
        [YamlMember(Alias = "overwrite", Description = "若补天已有的资源是否覆盖导入，若为 false，则只会导入补天没有的", ApplyNamingConventions = false)]
        public bool Overwrite { get; set; } = false;
        [YamlMember(Alias = "thread", Description = "导入的线程池大小，默认是你逻辑核心 * 2，如果太卡可以调小些", ApplyNamingConventions = false)]
        public int ThreadCount { get; set; } = 8;
    }
}
