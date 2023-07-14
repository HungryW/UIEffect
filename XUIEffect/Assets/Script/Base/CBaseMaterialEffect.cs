using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UIEffect
{
    [DisallowMultipleComponent]
    public abstract class CBaseMaterialEffect : CBaseMeshEffect, IParameterTexture, IMaterialModifier
    {
        protected static readonly Hash128 ms_InvalidHash = new Hash128();
        protected static readonly List<UIVertex> ms_listTempVerts = new List<UIVertex>();
        protected static readonly StringBuilder ms_stringBuilder = new StringBuilder();

        private Hash128 m_nEffectMaterialHash;

        public int ParamIdx { get; set; }

        public virtual CParameterTexture ParamTex
        {
            get { return null; }
        }

        public void SetMaterialDirty()
        {
            _connector.SetMaterialDirty(graphic);
        }

        public virtual Hash128 GetMaterialHash(Material a_baseMat)
        {
            return ms_InvalidHash;
        }

        public Material GetModifiedMaterial(Material a_baseMat)
        {
            return GetModifiedMaterial(a_baseMat, graphic);
        }

        public virtual Material GetModifiedMaterial(Material a_baseMat, Graphic a_graphic)
        {
            if (!isActiveAndEnabled)
            {
                return a_baseMat;
            }
            Hash128 oldHash = m_nEffectMaterialHash;
            m_nEffectMaterialHash = GetMaterialHash(a_baseMat);
            Material modifyMat = a_baseMat;
            if (m_nEffectMaterialHash.isValid)
            {
                modifyMat = CMaterialCache.Register(a_baseMat, m_nEffectMaterialHash, ModifyMaterial, a_graphic);
            }

            CMaterialCache.UnRegister(oldHash);
            return modifyMat;
        }

        public virtual void ModifyMaterial(Material a_newMat, Graphic a_graphic)
        {
            if (isActiveAndEnabled && ParamTex != null)
            {
                ParamTex.RegisterMaterial(a_newMat);
            }
        }

        protected void _SetShaderVariants(Material a_newMat, params object[] a_arrVariants)
        {
            var keyWords = a_arrVariants.Where((x) => { return (int)x > 0; })
                .Select(x => x.ToString().ToUpper())
                .Concat(a_newMat.shaderKeywords)
                .Distinct()
                .ToArray();

            a_newMat.shaderKeywords = keyWords;

            ms_stringBuilder.Length = 0;
            ms_stringBuilder.Append(Path.GetFileName(a_newMat.shader.name));
            foreach (var key in keyWords)
            {
                ms_stringBuilder.Append("-");
                ms_stringBuilder.Append(key);
            }
            a_newMat.name = ms_stringBuilder.ToString();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (ParamTex != null)
            {
                ParamTex.Register(this);
            }
            SetMaterialDirty();
            _SetEffectParamsDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SetMaterialDirty();
            if (ParamTex != null)
            {
                ParamTex.UnRegister(this);
            }
            CMaterialCache.UnRegister(m_nEffectMaterialHash);
            m_nEffectMaterialHash = ms_InvalidHash;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            SetMaterialDirty();
            _SetVerticesDirty();
            _SetEffectParamsDirty();
        }

        protected override void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            _SetVerticesDirty();
            _SetEffectParamsDirty();
        }
#endif
    }
}


