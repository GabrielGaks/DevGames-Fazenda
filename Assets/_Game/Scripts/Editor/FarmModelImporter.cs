#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DevGames.Editor
{
    /// <summary>
    /// Preserves the coordinate conversion authored inside third-party FBX files.
    /// Quaternius models already target Unity; baking their axes again lays props sideways.
    /// </summary>
    public sealed class FarmModelImporter : AssetPostprocessor
    {
        const string ThirdPartyRoot = "Assets/_Game/ThirdParty/";

        /// <summary>
        /// Configures model-axis conversion before Unity imports an FBX. Unity invokes
        /// this callback automatically, so it has no arguments or return value.
        /// </summary>
        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ThirdPartyRoot) || !assetPath.EndsWith(".fbx")) return;
            ((ModelImporter)assetImporter).bakeAxisConversion = false;
        }

        /// <summary>
        /// The styloo Cozy Farm palette washes out under glossy PBR; its author recommends a
        /// rough, matte look, so its imported materials get almost no smoothness.
        /// </summary>
        void OnPostprocessMaterial(Material material)
        {
            if (assetPath.StartsWith(ThirdPartyRoot + "Styloo/")) material.SetFloat("_Smoothness", 0.1f);
        }
    }
}
#endif
