using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem.Gasify
{
        // Assets/Editor/MakeIconsSolidColor.cs
    #if UNITY_EDITOR
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class MakeIconsSolidColor
    {
        [MenuItem("Tools/Gasify/Icons/Make Selected Icons White (keep alpha)")]
        public static void MakeSelectedIconsWhite() => RecolorSelected(new Color32(255,255,255,255), "_white");

        [MenuItem("Tools/Gasify/Icons/Make Selected Icons…")]
        public static void MakeSelectedIconsSolidColor()
        {
            var html = EditorUtility.DisplayDialogComplex(
                "Make Icons Solid Color",
                "Enter an HTML color (e.g. #FFFFFF or #80FF00) in the next step.\n" +
                "All selected textures will be processed; RGB is replaced with that color and alpha is preserved.",
                "Continue", "Cancel", "White");

            if (html == 1) return; // Cancel
            var color = Color.white;
            if (html == 2) { RecolorSelected(color, "_white"); return; }

            var value = EditorUtility.DisplayDialogComplex("Color Input", "Use #RRGGBB or #RRGGBBAA", "OK", "Cancel", "Help");
            if (value == 2)
            {
                EditorUtility.DisplayDialog("Help", "Examples:\n#FFFFFF (white)\n#000000 (black)\n#FF00FF (magenta)", "OK");
                return;
            }
            if (value == 1) return;

            var input = EditorUtility.DisplayDialogComplex("Paste Color", "Paste your hex in the console and press OK", "OK", "Cancel", "Use #FFFFFF");
            if (input == 2) { color = Color.white; }
            // For simplicity, fallback to white. If you want a text prompt, replace the above with an EditorWindow text field.
            RecolorSelected(color, "_solid");
        }

        static void RecolorSelected(Color targetRgb, string suffix)
        {
            var guids = Selection.assetGUIDs.Where(g =>
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var type = AssetDatabase.GetMainAssetTypeAtPath(p);
                return type == typeof(Texture2D);
            }).ToArray();

            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Make Icons Solid Color", "Select one or more Texture2D assets (PNGs).", "OK");
                return;
            }
            
            int ok = 0, fail = 0;
            foreach (var guid in guids)
            {
                Debug.Log(guid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!ProcessPath(path, targetRgb, suffix)) fail++; else ok++;
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", $"Processed: {ok}\nFailed: {fail}", "OK");
        }

        static bool ProcessPath(string path, Color targetRgb, string suffix)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            // Remember importer settings to restore if desired
            var prevReadable = importer.isReadable;
            var prevCompression = importer.textureCompression;
            var prevSRGB = importer.sRGBTexture;

            try
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) return false;

                // Read pixels
                var pixels = tex.GetPixels32();
                // Build recolored
                var outTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false, false);
                var r = (byte)Mathf.RoundToInt(targetRgb.r * 255f);
                var g = (byte)Mathf.RoundToInt(targetRgb.g * 255f);
                var b = (byte)Mathf.RoundToInt(targetRgb.b * 255f);

                for (int i = 0; i < pixels.Length; i++)
                {
                    var p = pixels[i];
                    // Keep original alpha (or use max of RGB & A if your alpha is empty):
                    // byte a = (byte)Mathf.Max(p.r, Mathf.Max(p.g, Mathf.Max(p.b, p.a)));
                    byte a = p.a;
                    pixels[i] = new Color32(r, g, b, a);
                }

                outTex.SetPixels32(pixels);
                outTex.Apply(false, true);

                Debug.Log(outTex);

                // Write as a new asset (non-destructive)
                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path) + suffix + ".png";
                var newPath = Path.Combine(dir, name).Replace("\\", "/");
                File.WriteAllBytes(newPath, outTex.EncodeToPNG());

                // Import with Editor-friendly settings
                AssetDatabase.ImportAsset(newPath);
                var newImp = (TextureImporter)AssetImporter.GetAtPath(newPath);
                newImp.textureType = TextureImporterType.GUI;              // “Editor GUI and Legacy GUI”
                newImp.alphaIsTransparency = true;
                newImp.mipmapEnabled = false;
                newImp.textureCompression = TextureImporterCompression.Uncompressed;
                newImp.sRGBTexture = true;
                newImp.SaveAndReimport();

                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.StackTrace);
                return false;
            }
            finally
            {
                // Restore original importer if you want (optional)
                importer.isReadable = prevReadable;
                importer.textureCompression = prevCompression;
                importer.sRGBTexture = prevSRGB;
                importer.SaveAndReimport();
            }
        }
    }
    #endif

}
