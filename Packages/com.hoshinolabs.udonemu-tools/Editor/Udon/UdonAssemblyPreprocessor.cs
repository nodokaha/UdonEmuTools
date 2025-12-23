using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEngine;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace HoshinoLabs.UdonEmuTools.Udon {
    internal class UdonAssemblyPreprocessor {
        string[] programs;
        int cursor;

        public static bool TryPreprocessAssembly(string uassembly, ref IUdonHeap heap, out string result) {
            var preprocessor = new UdonAssemblyPreprocessor();
            preprocessor.programs = uassembly.Split('\n');
            if (!preprocessor.TryParseBlock(ref heap)) {
                result = default;
                return false;
            }
            result = string.Join('\n', preprocessor.programs);
            return true;
        }

        bool TryParseBlock(ref IUdonHeap heap) {
            while (!EoL()) {
                var l = Next();
                switch (l) {
                    case ".data_start": {
                            if (!TryParseDataBlock(ref heap)) {
                                return false;
                            }
                            break;
                        }
                    case ".code_start": {
                            if (!TryParseCodeBlock()) {
                                return false;
                            }
                            break;
                        }
                }
            }
            return true;
        }

        bool TryParseDataBlock(ref IUdonHeap heap) {
            var pointer = 0u;
            while (!EoL()) {
                var l = Next();
                switch (l) {
                    case ".data_end": {
                            return true;
                        }
                    default: {
                            if (l.StartsWith(".export")) {
                                if (!TryParseDataExport(l)) {
                                    return false;
                                }
                                break;
                            }
                            if (!TryParseDataDeclaration(l, ref pointer, ref heap)) {
                                return false;
                            }
                            break;
                        }
                }
            }
            Debug.LogError($"Unexpected EoL. Expected '.data_end', Line: {cursor}.");
            return false;
        }

        bool TryParseDataExport(string l) {
            var m = Regex.Match(l, @"\.export (\w+)");
            if (!m.Success || m.Groups.Count != 2) {
                Debug.LogError($"Unrecognized Line in data block: {l}, Line: {cursor}.");
                return false;
            }
            return true;
        }

        bool TryParseDataDeclaration(string l, ref uint pointer, ref IUdonHeap heap) {
            if (!TryParseDataDeclarationSymbol(l)) {
                return false;
            }
            if (!TryParseDataDeclarationLiteral(l, ref pointer, ref heap)) {
                return false;
            }
            return true;
        }

        bool TryParseDataDeclarationSymbol(string l) {
            var m = Regex.Match(l, @"(\w+):");
            if (!m.Success || m.Groups.Count != 2) {
                Debug.LogError($"Unrecognized Line in data block: {l}, Line: {cursor}.");
                return false;
            }
            return true;
        }

        bool TryParseDataDeclarationLiteral(string l, ref uint pointer, ref IUdonHeap heap) {
            var m = Regex.Match(l, @"%(\w+?), *(.+)");
            if (!m.Success || m.Groups.Count != 3) {
                Debug.LogError($"Unrecognized Line in data block: {l}, Line: {cursor}.");
                return false;
            }
            var _v = m.Groups[2].Captures[0].Value;
            var heapValue = heap.GetHeapVariable(pointer);
            if (heapValue != null && heapValue is not UdonGameObjectComponentHeapReference) {
                _v = TextSerializer.Serialize(heapValue);
                if (!double.TryParse(heapValue.ToString(), out var _)) {
                    _v = JsonConvert.ToString(_v);
                }
            }
            programs[cursor - 1] = $"    {l.Substring(0, m.Groups[2].Index)}{_v}";
            pointer++;
            return true;
        }

        bool TryParseCodeBlock() {
            while (!EoL()) {
                var l = Next();
                switch (l) {
                    case ".code_end": {
                            return true;
                        }
                    default: {
                            if (l.StartsWith(".export")) {
                                if (!TryParseCodeExport(l)) {
                                    return false;
                                }
                                break;
                            }
                            if (l.EndsWith(":")) {
                                if (!TryParseCodeLabel(l)) {
                                    return false;
                                }
                                break;
                            }
                            if (!TryParseCodeInstruction(l)) {
                                return false;
                            }
                            break;
                        }
                }
            }
            Debug.LogError($"Unexpected EOF. Expected '.code_end', Line: {cursor}.");
            return false;
        }

        bool TryParseCodeExport(string l) {
            var m = Regex.Match(l, @"\.export (\w+)");
            if (!m.Success || m.Groups.Count != 2) {
                Debug.LogError($"Unrecognized Line in data block: {l}, Line: {cursor}.");
                return false;
            }
            return true;
        }

        bool TryParseCodeLabel(string l) {
            var m = Regex.Match(l, @"(\w+):");
            if (!m.Success || m.Groups.Count != 2) {
                Debug.LogError($"Unrecognized Line in code block: {l}, Line: {cursor}.");
                return false;
            }
            return true;
        }

        bool TryParseCodeInstruction(string l) {
            var m = Regex.Match(l, @"([A-Z_]+)(?:, (.+))?");
            if (!m.Success || m.Groups.Count != 3) {
                Debug.LogError($"Unrecognized Line in code block: {l}, Line: {cursor}.");
                return false;
            }
            return true;
        }

        bool EoL() {
            return programs.Length <= cursor;
        }

        string Next() {
            while (!EoL()) {
                var l = programs[cursor++];
                var c = l.IndexOf('#');
                if (0 <= c) {
                    l = l.Substring(0, c);
                }
                l = l.Trim();
                if (!string.IsNullOrEmpty(l)) {
                    return l;
                }
            }
            return null;
        }
    }
}
