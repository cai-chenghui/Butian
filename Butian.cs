using MelonLoader;
using System.Diagnostics;
using System.IO;

namespace Butian
{
    public class Butian : MelonMod
    {
        internal static Config Config;
        private bool prefetched = false;

        public override void OnApplicationStart()
        {
            Config = Config.LoadConfig();
            if (Config?.Hylas?.Import == true)
            {
                var sw = new Stopwatch();
                MelonLogger.Msg("Start importing from Hylas.");
                sw.Start();
                HylasHelper.Import();
                sw.Stop();
                MelonLogger.Msg($"Importing completed, takes: {sw.Elapsed}");
                Config.Hylas.Import = false;
                Config.Save();
            }
            ResourcesLoadPatch.Patch(Harmony);
            Assets.Scan();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex != 0) return;
            if (prefetched) return;
            var sw = new Stopwatch();
            MelonLogger.Msg("Start prefetching.");
            sw.Start();
            Assets.Prefetch();
            sw.Stop();
            MelonLogger.Msg($"Prefetching completed, takes: {sw.Elapsed}");
            prefetched = true;
        }

        public static string GetHome()
        {
            var home = Path.Combine(MelonUtils.GameDirectory, "Mods", nameof(Butian));
            if (!home.Exists())
            {
                Directory.CreateDirectory(home);
            }
            return home;
        }

        public static string GetAssetsPath()
        {
            var assetsPath = Path.Combine(GetHome(), "Assets");
            if (!assetsPath.Exists())
            {
                Directory.CreateDirectory(assetsPath);
            }
            return assetsPath;
        }
    }
}
