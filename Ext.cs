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
using System;
using System.IO;
using UnhollowerBaseLib;
using Object = UnityEngine.Object;

namespace Butian
{
    internal static class Ext
    {
        private delegate IntPtr Load(IntPtr path, IntPtr systemTypeInstance);

        private static readonly Load _load;

        static Ext()
        {
            _load = IL2CPP.ResolveICall<Load>("UnityEngine.Resources::Load(System.String,System.Type)");
        }

        public static Object ResourcesLoad(this string path, Il2CppSystem.Type systemTypeInstance)
        {
            var ptr = _load(IL2CPP.ManagedStringToIl2Cpp(path), IL2CPP.Il2CppObjectBaseToPtrNotNull(systemTypeInstance));
            return ptr != IntPtr.Zero ? new Object(ptr) : null;
        }

        /// <summary>
        /// 验证文件夹或者文件是否存在
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <returns>true 存在，false 不存在</returns>
        public static bool Exists(this string path)
        {
            return Directory.Exists(path) | File.Exists(path);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException(nameof(fromPath));
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException(nameof(toPath));

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string GetResourcePath(this string physicalPath)
        {
            return physicalPath.GetResourcePath(Butian.GetAssetsPath());
        }

        public static string GetResourcePath(this string physicalPath, string basePath)
        {
            return MakeRelativePath(basePath + "\\", Path.GetDirectoryName(physicalPath)).Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
