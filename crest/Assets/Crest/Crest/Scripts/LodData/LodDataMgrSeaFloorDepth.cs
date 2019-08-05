﻿// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    /// <summary>
    /// Renders depth of the ocean (height of sea level above ocean floor), by rendering the relative height of tagged objects from top down.
    /// </summary>
    public class LodDataMgrSeaFloorDepth : LodDataMgr
    {
        public override string SimName { get { return "SeaFloorDepth"; } }
        public override RenderTextureFormat TextureFormat { get { return RenderTextureFormat.RFloat; } }
        protected override bool NeedToReadWriteTextureData { get { return false; } }

        public override SimSettingsBase CreateDefaultSettings() { return null; }
        public override void UseSettings(SimSettingsBase settings) { }

        bool _targetsClear = false;
        private static int sp_SliceViewProjMatrices = Shader.PropertyToID("_SliceViewProjMatrices");
        private static int sp_CurrentLodCount = Shader.PropertyToID("_CurrentLodCount");
        private const string ENABLE_GEOMETRY_SHADER_KEYWORD = "_ENABLE_GEOMETRY_SHADER";

        public static string ShaderName
        {
            get
            {
                // TODO(SRT): Delete geometry shader version of this
                return "Crest/Inputs/Depth/Cached Depths";
            }
        }

        public override void BuildCommandBuffer(OceanRenderer ocean, CommandBuffer buf)
        {
            base.BuildCommandBuffer(ocean, buf);

            // if there is nothing in the scene tagged up for depth rendering, and we have cleared the RTs, then we can early out
            var drawList = RegisterLodDataInputBase.GetRegistrar(GetType());
            if (drawList.Count == 0 && _targetsClear)
            {
                return;
            }


            for (int lodIdx = OceanRenderer.Instance.CurrentLodCount - 1; lodIdx >= 0; lodIdx--)
            {
                buf.SetRenderTarget(_targets[lodIdx], 0);
                buf.ClearRenderTarget(false, true, Color.white * 1000f);
                buf.SetGlobalFloat(OceanRenderer.sp_LD_SliceIndex, lodIdx);
                SubmitDraws(lodIdx, buf);
            }

            // targets have now been cleared, we can early out next time around
            if (drawList.Count == 0)
            {
                _targetsClear = true;
            }
        }

        public static string TextureArrayName = "_LD_Texture_SeaFloorDepth";
        private static TextureArrayParamIds textureArrayParamIds = new TextureArrayParamIds(TextureArrayName);
        public static int ParamIdSampler(LodIdType lodIdType = LodIdType.SmallerLod) { return textureArrayParamIds.GetId(lodIdType); }
        protected override int GetParamIdSampler(LodIdType lodIdType = LodIdType.SmallerLod)
        {
            return ParamIdSampler(lodIdType);
        }
        public static void BindNull(IPropertyWrapper properties, LodIdType lodIdType = LodIdType.SmallerLod)
        {
            properties.SetTexture(ParamIdSampler(lodIdType), Texture2D.blackTexture);
        }
    }
}
