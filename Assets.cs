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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Butian
{
    internal class Assets
    {
        public const string ASSET_FILE = "asset.yml";
        public const string CACHE_METHOD_FILE = ".cache.method";

        internal class Asset
        {
            private string path;
            private readonly CacheMethod cacheMethod;
            private bool cached = false;
            private Object asset;

            internal CacheMethod CacheMethod => cacheMethod;

            public Asset(string path, CacheMethod cacheMethod = CacheMethod.normal)
            {
                this.path = path;
                this.cacheMethod = cacheMethod;
            }

            internal void Update()
            {
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    asset = Bridger.Bridge(path);
                    cached = true & CacheMethod > CacheMethod.normal;
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"Cache failed {path}\nreason: {e}");
                }
                sw.Stop();
                MelonDebug.Msg($"Time-consuming caching {path}: {sw.Elapsed}");
            }

            internal void Expire()
            {
                cached = false;
            }

            internal Object Get()
            {
                if (!cached)
                    Update();
                return asset ?? path.ResourcesLoad(Il2CppType.Of<Object>());

            }
        }

        private static Dictionary<string, Asset> _assetes = new Dictionary<string, Asset>();

        private static GameObject _root;

        private static readonly FileSystemWatcher _watcher = new FileSystemWatcher(Butian.GetAssetsPath());

        private static void OnError(object sender, ErrorEventArgs e)
        {
            MelonLogger.Warning(e);
        }

        private static void Handler(object sender, FileSystemEventArgs e)
        {
            if (!new Regex(@".*\.(yml)|(png)|(wav)]$").IsMatch(e.Name) && e.ChangeType != WatcherChangeTypes.Deleted)
                return;
            var path = e.FullPath.GetResourcePath();

            MelonLogger.Msg($"{path} {e.ChangeType} {e.Name}");
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Deleted:
                        Remove(path);
                        break;
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Renamed:
                        AddOrUpdate(path);
                        break;
                    case WatcherChangeTypes.All:
                        throw new NotSupportedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error(ex);
            }

        }

        public static GameObject CopyAsset(string path, GameObject go)
        {
            if (_assetes[path]?.CacheMethod > CacheMethod.normal)
                return Object.Instantiate(go, _root.transform);
            return Object.Instantiate(go);
        }

        public static bool TryGet(string path, out Object go)
        {
            if (!_assetes.ContainsKey(path))
            {
                go = null;
                return false;
            }
            try
            {
                var b = _assetes.TryGetValue(path, out var asset);
                go = asset?.Get();
                return b;
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"An exception was encountered while trying to get {path}: {e}");
                go = null;
                return false;
            }
        }

        private static void Remove(string path)
        {
            if (Path.Combine(Butian.GetAssetsPath(), path, ASSET_FILE).Exists())
            {
                return;
            }
            _assetes.Remove(path);
        }

        private static void AddOrUpdate(string path, CacheMethod cacheMethod = CacheMethod.normal)
        {
            if (!Path.Combine(Butian.GetAssetsPath(), path, ASSET_FILE).Exists())
            {
                return;
            }
            if (_assetes.ContainsKey(path))
            {
                _assetes[path].Expire();
            }
            else
            {
                try
                {
                    _assetes.Add(path, new Asset(path, cacheMethod));
                }
                catch (Exception e)
                {
                    MelonLogger.Warning($"Cache failed, path = {path}\nreason: {e}");
                }
            }
        }


        internal static void Scan()
        {
            var prefetchConfig = Butian.Config?.Prefetch;
            foreach (var file in Directory.EnumerateFiles(Butian.GetAssetsPath(), ASSET_FILE, SearchOption.AllDirectories))
            {
                var path = file.GetResourcePath();
                var cacheMethod = CacheMethod.normal;
                if (prefetchConfig.Enable)
                {
                    foreach (var regex in prefetchConfig.Whitelist)
                    {
                        if (regex.IsMatch(path))
                        {
                            cacheMethod = CacheMethod.prefetch;
                            break;
                        }
                    }
                }
                var cache_method_path = file.Replace(ASSET_FILE, CACHE_METHOD_FILE);
                if (cache_method_path.Exists())
                {
                    var str = File.ReadAllText(cache_method_path);
                    try
                    {
                        cacheMethod = (CacheMethod)Math.Max(int.Parse(str), (int)cacheMethod);
                        // 如果未开启预加载，则设置为 cache
                        cacheMethod = cacheMethod > CacheMethod.cahce && !prefetchConfig.Enable ? CacheMethod.cahce : cacheMethod;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Warning($"Unknown method {str}, set as default[{cacheMethod}]: {e.Message}");
                    }
                }

                MelonDebug.Msg($"Scan: {path} CacheMethod: {cacheMethod}");
                AddOrUpdate(path, cacheMethod);
            }
        }

        internal static int Prefetch()
        {
            _root = new GameObject
            {
                name = "Butian Assets",
                active = false
            };
            Object.DontDestroyOnLoad(_root);

            _watcher.NotifyFilter = NotifyFilters.Attributes
                                    | NotifyFilters.DirectoryName
                                    | NotifyFilters.FileName
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.Security
                                    | NotifyFilters.Size;

            _watcher.Changed += Handler;
            _watcher.Created += Handler;
            _watcher.Renamed += Handler;
            _watcher.Deleted += Handler;
            _watcher.Error += OnError;

            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.Filter = string.Format("{0}|*.png|*.wav", ASSET_FILE);

            var list = _assetes.Values.Where(i => i.CacheMethod == CacheMethod.prefetch);
            foreach (var item in list)
            {
                item.Update();
            }
            return list.Count();
        }
    }


    internal enum CacheMethod
    {
        prefetch = 2,
        cahce = 1,
        normal = 0
    }
}
