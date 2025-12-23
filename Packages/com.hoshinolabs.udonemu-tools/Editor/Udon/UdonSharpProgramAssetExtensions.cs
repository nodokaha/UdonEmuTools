using System.Reflection;
using UdonSharp;

namespace HoshinoLabs.UdonEmuTools.Udon {
    internal static class UdonSharpProgramAssetExtensions {
        public static string GetUdonAssembly(this UdonSharpProgramAsset self) {
            var assembly = typeof(UdonSharpProgramAsset).Assembly;
            var type = assembly.GetType("UdonSharp.UdonSharpEditorCache");
            var property = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var instance = property.GetValue(null);
            var method = type.GetMethod("GetUASMStr", BindingFlags.Public | BindingFlags.Instance);
            return (string)method.Invoke(instance, new[] { self });
        }
    }
}
