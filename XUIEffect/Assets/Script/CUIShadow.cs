using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIEffect
{
    public enum EShowdowStyle
    {
        None,
        Shadow,
        Shadow3,
        Outline,
        Outline8,
    }

    [RequireComponent(typeof(Graphic))]
    public class CUIShadow : CBaseMeshEffect, IParameterTexture
    {
        private static readonly List<CUIShadow> ms_listTempShadows = new List<CUIShadow>();
        private static readonly List<UIVertex> ms_listVerts = new List<UIVertex>(4096);
        private const float mc_fMaxEffectDistance = 600f;

        private int m_nGraghicVertexCount;
        [SerializeField]
        private EShowdowStyle m_eStyle = EShowdowStyle.Shadow;
        [SerializeField]
        private Color m_effectColor = Color.white;
        [SerializeField]
        private Vector2 m_v2EffecrDistance = new Vector2(1, -1);
        [SerializeField]
        private bool m_bUseGraphicAlpha = true;
        [SerializeField]
        [Range(0, 1)]
        private float m_fBlurFactor = 1;

        public int ParamIdx { get; set; }

        public CParameterTexture ParamTex { get; private set; }

        public Color effectColor
        {
            get
            {
                return m_effectColor;
            }
            set
            {
                if (m_effectColor == value)
                {
                    return;
                }
                m_effectColor = value;
                _SetVerticesDirty();
            }
        }

        public Vector2 effectDistance
        {
            get
            {
                return m_v2EffecrDistance;
            }
            set
            {
                if (m_v2EffecrDistance == value)
                {
                    return;
                }
                m_v2EffecrDistance = value;
                _SetEffectParamsDirty();
            }
        }

        public bool useGraphicAlpha
        {
            get
            {
                return m_bUseGraphicAlpha;
            }
            set
            {
                if (m_bUseGraphicAlpha == value)
                {
                    return;
                }
                m_bUseGraphicAlpha = value;
                _SetEffectParamsDirty();
            }
        }

        public float blurFactor
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

        public EShowdowStyle style
        {
            get
            {
                return m_eStyle;
            }
            set
            {
                if (m_eStyle == value)
                {
                    return;
                }
                m_eStyle = value;
                _SetEffectParamsDirty();
            }
        }

        public override void MondifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled || vh.currentIndexCount <= 0 || m_eStyle == EShowdowStyle.None)
            {
                return;
            }
            vh.GetUIVertexStream(ms_listVerts);

            GetComponents<CUIShadow>(ms_listTempShadows);

            foreach (var s in ms_listTempShadows)
            {
                if (!s.isActiveAndEnabled)
                {
                    continue;
                }
                if (s == this)
                {
                    foreach (var s2 in ms_listTempShadows)
                    {
                        s2.m_nGraghicVertexCount = ms_listVerts.Count;
                    }
                }
                break;
            }
            ms_listTempShadows.Clear();


            if (ParamTex != null)
            {
                ParamTex.SetData(this, 0, 0);
                ParamTex.SetData(this, 1, 255);
                ParamTex.SetData(this, 2, 255);
            }
            int nStart = ms_listVerts.Count - m_nGraghicVertexCount;
            int nEnd = ms_listVerts.Count;

            _ApplyShadow(ms_listVerts, m_effectColor, m_v2EffecrDistance, m_eStyle, m_bUseGraphicAlpha, ref nStart, ref nEnd);
           
            
            vh.Clear();
            vh.AddUIVertexTriangleStream(ms_listVerts);
            ms_listVerts.Clear();
        }

        private void _ApplyShadow(List<UIVertex> a_listVerts, Color a_color, Vector2 a_v2Distance, EShowdowStyle a_eStyle, bool a_bAlpha, ref int a_outnStart, ref int a_outnEnd)
        {
            if (a_eStyle == EShowdowStyle.None || a_color.a <= 0)
            {
                return;
            }

            float fX = a_v2Distance.x;
            float fY = a_v2Distance.y;
            _ApplyShadowZeroAlloc(a_listVerts, a_color, fX, fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);

            switch (a_eStyle)
            {
                case EShowdowStyle.Shadow3:
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, fX, 0, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, 0, fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    break;
                case EShowdowStyle.Outline:
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, -fX, fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, fX, -fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, -fX, -fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    break;
                case EShowdowStyle.Outline8:
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, -fX, fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, fX, -fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, -fX, -fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, fX, 0, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, 0, fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, -fX, 0, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    _ApplyShadowZeroAlloc(a_listVerts, a_color, 0, -fY, a_bAlpha, ref a_outnStart, ref a_outnEnd);
                    break;
            }
        }

        private void _ApplyShadowZeroAlloc(List<UIVertex> a_listVerts, Color a_color, float a_fX, float a_fY, bool a_bAlpha, ref int a_outnStart, ref int a_outnEnd)
        {
            int nCount = a_outnEnd - a_outnStart;
            int nNeedCapacity = a_listVerts.Count + nCount;
            if (a_listVerts.Capacity < nNeedCapacity)
            {
                a_listVerts.Capacity *= 2;
            }

            float fNormalTextureIdx = ParamTex != null ? ParamTex.GetNormalizedIdx(this) : -1;

            UIVertex vt = default(UIVertex);
            for (int i = 0; i < nCount; i++)
            {
                a_listVerts.Add(vt);
            }

            for (int n = a_listVerts.Count - 1; n >= nCount; n--)
            {
                a_listVerts[n] = a_listVerts[n - nCount];
            }

            for (int i = 0; i < nCount; i++)
            {
                vt = a_listVerts[i + a_outnStart + nCount];
                Vector3 vPos = vt.position;
                vt.position.Set(vPos.x + a_fX, vPos.y + a_fY, vPos.z);

                Color colorVt = a_color;
                colorVt.a = a_bAlpha ? a_color.a * vt.color.a / 255 : a_color.a;
                vt.color = colorVt;

                if (fNormalTextureIdx >= 0)
                {
                    vt.uv0 = new Vector2(vt.uv0.x, fNormalTextureIdx);
                }
                a_listVerts[i] = vt;
            }

            a_outnStart = a_outnEnd;
            a_outnEnd = a_listVerts.Count;
        }
    }
}


