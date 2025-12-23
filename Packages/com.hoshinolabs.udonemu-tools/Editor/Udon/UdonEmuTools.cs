using UdonSharp;
using UnityEditor;
using VRC.Udon;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram;

namespace HoshinoLabs.UdonEmuTools.Udon {
    internal static class UdonEmuTools {
        [MenuItem("Tools/HoshinoLabs/UdonEmu/Copy Assembly To Clipboard With Heap", priority = 18000)]
        [MenuItem("Assets/Copy Assembly To Clipboard With Heap", priority = 18000)]
        public static void CopyAssemblyToClipboardWithHeap() {
            if (!TryGetProgramSourceFromSelection(out var programSource)) {
                return;
            }
            if (!TryGetUdonAssemblyFromProgramSource(programSource, out var uassembly)) {
                return;
            }
            var program = programSource.SerializedProgramAsset.RetrieveProgram();
            var heap = program.Heap;
            if (!UdonAssemblyPreprocessor.TryPreprocessAssembly(uassembly, ref heap, out var result)) {
                return;
            }
            EditorGUIUtility.systemCopyBuffer = result;
        }

        [MenuItem("Tools/HoshinoLabs/UdonEmu/Copy Assembly To Clipboard With Heap", true)]
        [MenuItem("Assets/Copy Assembly To Clipboard With Heap", true)]
        public static bool ValidCopyAssemblyToClipboardWithHeap() {
            if (!TryGetProgramSourceFromSelection(out var programSource)) {
                return false;
            }
            return programSource.GetType() == typeof(UdonSharpProgramAsset)
                || programSource.GetType() == typeof(UdonAssemblyProgramAsset)
                || programSource.GetType() == typeof(UdonGraphProgramAsset);
        }

        static bool TryGetProgramSourceFromSelection(out UdonAssemblyProgramAsset programSource) {
            programSource = default;
            var activeObject = Selection.activeObject;
            if (activeObject != null) {
                programSource = (UdonAssemblyProgramAsset)activeObject;
            }
            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject != null) {
                var udonBehaviour = activeGameObject.GetComponent<UdonBehaviour>();
                programSource = (UdonAssemblyProgramAsset)udonBehaviour?.programSource;
            }
            return programSource != null;
        }

        static bool TryGetUdonAssemblyFromProgramSource(UdonAssemblyProgramAsset programSource, out string uassembly) {
            switch (programSource) {
                case UdonSharpProgramAsset udonSharpProgramAsset: {
                        uassembly = udonSharpProgramAsset.GetUdonAssembly();
                        return true;
                    }
                case UdonAssemblyProgramAsset udonAssemblyProgramAsset: {
                        uassembly = udonAssemblyProgramAsset.GetUdonAssembly();
                        return true;
                    }
            }
            uassembly = null;
            return false;
        }
    }
}
