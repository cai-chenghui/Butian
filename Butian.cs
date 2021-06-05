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
            ResourcesLoadPatch.Patch(Harmony);
            Assets.Scan();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex != 0) return;
            if (prefetched) return;
            var sw = new Stopwatch();
            MelonLogger.Msg("Prefetch start...");
            sw.Start();
            Assets.Prefetch();
            sw.Stop();
            MelonLogger.Msg($"Prefetch end, total: {sw.Elapsed}");
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
