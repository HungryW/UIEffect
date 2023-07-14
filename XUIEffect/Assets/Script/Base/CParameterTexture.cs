using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIEffect
{
    public interface IParameterTexture
    {
        int ParamIdx { get; set; }
        CParameterTexture ParamTex { get; }
    }
    [Serializable]
    public class CParameterTexture
    {
        private static List<Action> m_listUpdateCallbacks;

        private Texture2D m_texture;
        private bool m_bNeedUpload;
        private int m_nPropertyId;
        private readonly string m_szPropertyName;
        private readonly int m_nChannels;
        private readonly int m_nInstanceLimit;
        private readonly byte[] m_arrData;
        private readonly Stack<int> m_stack;

        public CParameterTexture(int a_nChannels, int a_nInstanceLimit, string a_szName)
        {
            m_szPropertyName = a_szName;
            //向上取整获得最近4的倍数
            m_nChannels = ((a_nChannels - 1) / 4 + 1) * 4;
            //向上取整获得最近2的倍数
            m_nInstanceLimit = ((a_nInstanceLimit - 1) / 2 + 1) * 2;
            m_arrData = new byte[m_nChannels * m_nInstanceLimit];
            m_stack = new Stack<int>(m_nInstanceLimit);
            for (int i = 1; i <= m_nInstanceLimit; i++)
            {
                m_stack.Push(i);
            }
        }

        public void Register(IParameterTexture a_target)
        {
            _Initialize();
            if (a_target.ParamIdx <= 0 && m_stack.Count > 0)
            {
                a_target.ParamIdx = m_stack.Pop();
            }
        }

        public void UnRegister(IParameterTexture a_target)
        {
            if (a_target.ParamIdx > 0)
            {
                m_stack.Push(a_target.ParamIdx);
                a_target.ParamIdx = 0;
            }
        }

        public void SetData(IParameterTexture a_target, int a_nChannelId, byte value)
        {
            int nIdx = (a_target.ParamIdx - 1) * m_nChannels + a_nChannelId;
            if (a_target.ParamIdx > 0 && m_arrData[nIdx] != value)
            {
                m_arrData[nIdx] = value;
                m_bNeedUpload = true;
            }
        }

        public void SetData(IParameterTexture a_target, int a_nChannelId, float a_val)
        {
            SetData(a_target, a_nChannelId, (byte)(Mathf.Clamp01(a_val) * 255));
        }

        public void RegisterMaterial(Material mat)
        {
            if (m_nPropertyId == 0)
            {
                m_nPropertyId = Shader.PropertyToID(m_szPropertyName);
            }
            if (mat != null)
            {
                mat.SetTexture(m_nPropertyId, m_texture);
            }
        }

        public float GetNormalizedIdx(IParameterTexture a_target)
        {
            return ((float)a_target.ParamIdx - 0.5f) / m_nInstanceLimit;
        }

        private void _Initialize()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            if (m_listUpdateCallbacks == null)
            {
                m_listUpdateCallbacks = new List<Action>();
                Canvas.willRenderCanvases += () =>
                {
                    foreach (var fn in m_listUpdateCallbacks)
                    {
                        fn.Invoke();
                    }
                };
            }
            if (m_texture == null)
            {
                bool isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;
                m_texture = new Texture2D(m_nChannels / 4, m_nInstanceLimit, TextureFormat.ARGB32, false, isLinear);
                m_texture.filterMode = FilterMode.Point;
                m_texture.wrapMode = TextureWrapMode.Clamp;

                m_listUpdateCallbacks.Add(_UpdateParameterTexture);
                m_bNeedUpload = true;
            }
        }

        private void _UpdateParameterTexture()
        {
            if (m_bNeedUpload && m_texture != null)
            {
                m_bNeedUpload = false;
                m_texture.LoadRawTextureData(m_arrData);
                m_texture.Apply(false, false);
            }
        }

    }
}


