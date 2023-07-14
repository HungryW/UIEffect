using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIEffect
{
    public partial class CGraphicConnector
    {
        private static readonly List<CGraphicConnector> ms_listConnectors = new List<CGraphicConnector>();
        private static readonly Dictionary<Type, CGraphicConnector> ms_mapConnectors = new Dictionary<Type, CGraphicConnector>();

        private static readonly CGraphicConnector ms_empty = new CGraphicConnector();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void _Init()
        {

        }

        protected static void _AddConnector(CGraphicConnector a_connector)
        {
            ms_listConnectors.Add(a_connector);
        }

        public static CGraphicConnector Find(Graphic graphic)
        {
            if (!graphic)
            {
                return ms_empty;
            }

            Type t = graphic.GetType();
            CGraphicConnector connector = null;
            if (ms_mapConnectors.TryGetValue(t, out connector))
            {
                return connector;
            }

            foreach (var c in ms_listConnectors)
            {
                if (c.IsValid(graphic))
                {
                    ms_mapConnectors.Add(t, c);
                    return c;
                }
            }
            return ms_empty;
        }
    }

    public partial class CGraphicConnector
    {
        protected virtual int Priority { get { return 0; } }

        public virtual AdditionalCanvasShaderChannels ExtraChannel
        {
            get { return AdditionalCanvasShaderChannels.TexCoord1; }
        }

        protected virtual bool IsValid(Graphic a_Graphic)
        {
            return true;
        }

        public virtual Shader FindShader(string a_szShaderName)
        {
            return Shader.Find("Hidden/" + a_szShaderName);
        }

        public virtual void OnEnable(Graphic a_Graphic)
        {

        }

        public virtual void OnDisable(Graphic a_Graphic)
        {

        }

        public virtual void SetVerticesDirty(Graphic graphics)
        {
            if (graphics)
            {
                graphics.SetVerticesDirty();
            }
        }

        public virtual void SetMaterialDirty(Graphic graphic)
        {
            if (graphic)
            {
                graphic.SetMaterialDirty();
            }
        }

        public virtual void GetPositionFactor(EEffectArea area, int index, Rect rect, Vector2 position, out float x, out float y)
        {
            if (area == EEffectArea.Fit)
            {
                x = Mathf.Clamp01((position.x - rect.xMin) / rect.width);
                y = Mathf.Clamp01((position.y - rect.yMin) / rect.height);
            }
            else
            {
                x = Mathf.Clamp01(position.x / rect.width + 0.5f);
                y = Mathf.Clamp01(position.y / rect.height + 0.5f);
            }
        }

        public virtual bool IsText(Graphic graphic)
        {
            return graphic && graphic is Text;
        }

        public virtual void SetExtraChannel(ref UIVertex vertex, Vector2 val)
        {
            vertex.uv1 = val;
        }

        public virtual void GetNormalizedFactor(EEffectArea area, int index, Matrix2x3 matrix, Vector2 position,
        out Vector2 normalizedPos)
        {
            normalizedPos = matrix * position;
        }
    }

    public enum EEffectArea
    {
        RectTransform,
        Fit,
        Character,
    }

    public struct Matrix2x3
    {
        public float m00, m01, m02, m10, m11, m12;

        public Matrix2x3(Rect rect, float cos, float sin)
        {
            const float center = 0.5f;
            float dx = -rect.xMin / rect.width - center;
            float dy = -rect.yMin / rect.height - center;
            m00 = cos / rect.width;
            m01 = -sin / rect.height;
            m02 = dx * cos - dy * sin + center;
            m10 = sin / rect.width;
            m11 = cos / rect.height;
            m12 = dx * sin + dy * cos + center;
        }

        public static Vector2 operator *(Matrix2x3 m, Vector2 v)
        {
            return new Vector2(
                (m.m00 * v.x) + (m.m01 * v.y) + m.m02,
                (m.m10 * v.x) + (m.m11 * v.y) + m.m12
            );
        }
    }
}
