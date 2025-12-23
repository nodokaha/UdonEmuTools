using System.Reflection;
using VRC.Udon.Editor.ProgramSources;

namespace HoshinoLabs.UdonEmuTools.Udon {
    internal static class UdonAssemblyProgramAssetExtensions {
        public static string GetUdonAssembly(this UdonAssemblyProgramAsset self) {
            var type = typeof(UdonAssemblyProgramAsset);
            var field = type.GetField("udonAssembly", BindingFlags.NonPublic | BindingFlags.Instance);
            return (string)(field.GetValue(self));
        }
    }
}
