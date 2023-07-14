using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIEffect
{

    public enum EEffectMode
    {
        None = 0,
        Grayscale,
        Speia,
        Nega,
        Pixel
    }

    public enum EColorMode
    {
        Multiply = 0,
        Fill,
        Add,
        Subtract,
    }

    public enum EBlurMode
    {
        None = 0,
        FastBlur,
        MediumBlur,
        DetailBlur,
    }



    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    public class CUIEffect : CBaseMaterialEffect, IMaterialModifier
    {
        private const uint mc_nShaderId = 2 << 3;
        private static readonly CParameterTexture ms_paramTex = new CParameterTexture(4, 1024, "_ParamTex");

        [SerializeField]
        [Range(0f, 1f)]
        private float m_fEffectFactor = 1;

        [SerializeField]
        [Range(0, 1f)]
        private float m_fColorFactor = 1;

        [SerializeField]
        [Range(0, 1f)]
        private float m_fBlurFactor = 1;

        [SerializeField]
        private EEffectMode m_eEffectMode = EEffectMode.None;

        [SerializeField]
        private EColorMode m_eColorMode = EColorMode.Multiply;

        [SerializeField]
        private EBlurMode m_eBlurMode = EBlurMode.None;

        [SerializeField]
        private bool m_bAdvanceBlur = false;

        private enum EBulrEx
        {
            None = 0,
            Ex = 1,
        }

        public AdditionalCanvasShaderChannels _uvMaskChannel
        {
            get { return _connector.ExtraChannel; }
        }

        public float _effectFactor
        {
            get
            {
                return m_fEffectFactor;
            }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_fEffectFactor, value))
                {
                    return;
                }
                m_fEffectFactor = value;
                _SetEffectParamsDirty();
            }
        }


        public float _colorFactor
        {
            get
            {
                return m_fColorFactor;
            }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_fColorFactor, value))
                {
                    return;
                }
                m_fColorFactor = value;
                _SetEffectParamsDirty();
            }
        }

        public float _blurFactor
        {
            get
            {
                return m_fBlurFactor;
            }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_fBlurFactor, value))
                {
                    return;
                }
                m_fBlurFactor = value;
                _SetEffectParamsDirty();
            }
        }

        public EEffectMode _effectMode
        {
            get
            {
                return m_eEffectMode;
            }
            set
            {
                if (m_eEffectMode == value)
                {
                    return;
                }
                m_eEffectMode = value;
                SetMaterialDirty();
            }
        }

        public EColorMode _colorMode
        {
            get
            {
                return m_eColorMode;
            }
            set
            {
                if (m_eColorMode == value)
                {
                    return;
                }
                m_eColorMode = value;
                SetMaterialDirty();
            }
        }

        public EBlurMode _blurMode
        {
            get
            {
                return m_eBlurMode;
            }
            set
            {
                if (m_eBlurMode == value)
                {
                    return;
                }
                m_eBlurMode = value;
                SetMaterialDirty();
            }
        }

        public bool _advancedBlur
        {
            get
            {
                return m_bAdvanceBlur;
            }
            set
            {
                if (m_bAdvanceBlur == value)
                {
                    return;
                }
                m_bAdvanceBlur = value;
                _SetVerticesDirty();
                SetMaterialDirty();
            }
        }


        public override CParameterTexture ParamTex
        {
            get
            {
                return ms_paramTex;
            }
        }


        public override Hash128 GetMaterialHash(Material a_baseMat)
        {
            if (!isActiveAndEnabled || !a_baseMat || !a_baseMat.shader)
            {
                return ms_InvalidHash;
            }

            uint nShaderVariantId = (uint)(((int)m_eEffectMode << 6)
                                        + ((int)m_eColorMode << 9)
                                        + ((int)m_eBlurMode << 11)
                                        + ((m_bAdvanceBlur ? 1 : 0) << 13));
            return new Hash128((uint)a_baseMat.GetInstanceID()
                                , mc_nShaderId + nShaderVariantId
                                , 0
                                , 0);
        }

        public override void ModifyMaterial(Material a_newMat, Graphic a_graphic)
        {
            CGraphicConnector connector = CGraphicConnector.Find(a_graphic);

            a_newMat.shader = Shader.Find(String.Format("Hidden/{0} (UIEffect)", a_newMat.shader.name));
            _SetShaderVariants(a_newMat, m_eEffectMode, m_eColorMode, m_bAdvanceBlur ? EBulrEx.Ex : EBulrEx.None);

            ParamTex.RegisterMaterial(a_newMat);
        }

        public override void ModifyMesh(VertexHelper vh, Graphic graphics)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            float fNormalIdx = ParamTex.GetNormalizedIdx(this);
            if (m_eBlurMode != EBlurMode.None && m_bAdvanceBlur)
            {
                vh.GetUIVertexStream(ms_listTempVerts);
                vh.Clear();

                int nCount = ms_listTempVerts.Count;

                int nBundleSize = _connector.IsText(graphic) ? 6 : nCount;
                Rect rectPosBound = default(Rect);
                Rect rectUvBound = default(Rect);
                Vector3 v3Size = Vector3.zero;
                Vector3 v3Pos = Vector3.zero;
                Vector3 v3Uv = Vector3.zero;

                float fExpand = (float)m_eBlurMode * 6 * 2;

            }
        }

     
    }
}

