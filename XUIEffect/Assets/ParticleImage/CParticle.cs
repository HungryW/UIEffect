using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ParticleImage
{
    public class CParticle
    {
        private CParticleImage m_refParImg;

        private Vector2 m_v2ModifiedPosition;
        private Vector2 m_v2Position;

        private Vector2 m_v2StartVelocity;
        private Vector2 m_v2NoiseVelocity;
        private Vector2 m_v2VeloVelocity;
        private Vector2 m_v2GravityVelocity;
        private Vector2 m_v2FinalVelocity;

        private Vector3 m_v3StartRotation;
        private Vector3 m_v3StartSize;
        private Vector3 m_v3Size;

        private Color m_Color;
        private Color m_StartColor;

        private float m_fTime;
        private float m_fLifeTime;

        private Transform m_Transform;
        private List<SpriteSheet> m_listSheets;

        private float m_fSizeLerp;
        private float m_fColorLerp;
        private float m_fRotateLerp;
        private float m_fGravityLerp;
        private float m_fVelocityXLerp;
        private float m_fVelocityYLerp;
        private float m_fSpeedLerp;
        private float m_fAttractorLerp;
        private float m_fVortexLerp;
        private float m_fStartFrameLerp;
        private float m_fFrameOverTimeLerp;
        private float m_fRatioRandom;

        private Vector3 m_v3Rot;

        private Vector2 m_v2AttractorTargetPos;

        private Vector3 m_v3LastPos;
        private Quaternion m_qLastRotation;
        private Vector3 m_v3DeltaRotation;

        private Vector2 m_v2LastPos;
        private Vector2 m_v2DeltaPos;

        private Vector2 m_v2TrailLastPos;
        private Vector2 m_v2TrailDeltaPos;

        private Vector2 m_v2LastPoint;
        private Vector3 m_v3Direction;

        private float m_fFrameDelta;
        private int m_nFrameId;
        private int m_nSheetId;

        private List<TrialPoint> m_listTrailPos;
        private VertexHelper m_vhTrail;

        public CParticle(CParticleImage a_refParImg, Vector2 a_v2Pos, Vector3 a_v3Rot, Vector2 a_velocity, Color a_color, Vector3 a_v3Size, float a_fLife)
        {
            m_refParImg = a_refParImg;

            m_v2Position = a_v2Pos;
            m_v2StartVelocity = a_velocity;
            m_StartColor = a_color;
            m_v3StartSize = a_v3Size;
            m_v3StartRotation = a_v3Rot;

            m_fLifeTime = a_fLife;

            m_v3LastPos = m_Transform.position;

            m_v2LastPoint = a_v2Pos;
            m_v2ModifiedPosition = a_v2Pos;
            m_v2LastPos = a_v2Pos;
            m_v2TrailLastPos = a_v2Pos;

            m_fSizeLerp = Random.value;
            m_fColorLerp = Random.value;
            m_fRotateLerp = Random.value;
            m_fAttractorLerp = Random.value;
            m_fGravityLerp = Random.value;
            m_fVortexLerp = Random.value;
            m_fStartFrameLerp = Random.value;
            m_fFrameOverTimeLerp = Random.value;
            m_fVelocityXLerp = Random.value;
            m_fVelocityYLerp = Random.value;
            m_fSpeedLerp = Random.value;
            m_fRatioRandom = Random.value;

            m_v2AttractorTargetPos = new Vector2(Random.value, Random.value);

            m_listSheets = new List<SpriteSheet>();

            m_vhTrail = new VertexHelper();
            m_listTrailPos = new List<TrialPoint>();
        }

        public void Animate()
        {
            float fDeltaTime = (m_refParImg.m_eTimeScale == ETimeScale.Normal) ? Time.deltaTime : Time.unscaledDeltaTime;
            m_fTime += fDeltaTime;
            if (m_fTime > m_fLifeTime)
            {
                return;
            }
            float fTimeProgress = m_fTime.Remap(0, m_fLifeTime, 0, 1);

            m_v2FinalVelocity = m_v2StartVelocity * m_refParImg.m_curveSpeedOverTime.Evaluate(fTimeProgress, m_fSpeedLerp);

            if (m_refParImg.m_eSpace == ESimulation.World)
            {
                Vector2 v2OffsetPos = m_Transform.InverseTransformPoint(m_v2LastPoint);
                m_v2ModifiedPosition += v2OffsetPos;

                m_v3DeltaRotation = Quaternion.Inverse(m_refParImg.transform.rotation).eulerAngles - Quaternion.Inverse(m_qLastRotation).eulerAngles;

                m_v2ModifiedPosition = _RatatePointAroundCenter(m_v2ModifiedPosition, m_v3DeltaRotation);
                m_v2StartVelocity = _RatatePointAroundCenter(m_v2StartVelocity, m_v3DeltaRotation);

                m_v2LastPos = m_Transform.position;
                m_qLastRotation = m_Transform.rotation;
            }


            #region Velocity
            if (m_refParImg.m_VelocityModule.m_bEnabled)
            {
                if (m_refParImg.m_eVelocitySpace == ESimulation.World)
                {

                }
                else
                {
                    if (m_refParImg.m_VelocityOverLifeTime.m_bSeparated)
                    {
                        float fx = m_refParImg.m_VelocityOverLifeTime.m_curveX.Evaluate(fTimeProgress, m_fVelocityXLerp);
                        float fy = m_refParImg.m_VelocityOverLifeTime.m_curveY.Evaluate(fTimeProgress, m_fVelocityYLerp);
                        m_v2VeloVelocity = new Vector2(fx, fy);
                    }
                    else
                    {
                        float fx = m_refParImg.m_VelocityOverLifeTime.m_curveMain.Evaluate(fTimeProgress, m_fVelocityXLerp);
                        m_v2VeloVelocity = new Vector2(fx, 0);
                    }
                }
            }
            #endregion

            #region Gravity
            if (m_refParImg.m_GravityModule.m_bEnabled)
            {
                float fVal = m_refParImg.m_curveGravity.Evaluate(fTimeProgress, m_fGravityLerp);
                Vector3 pos = new Vector3(0, fVal, 0);
                Vector3 angle = Quaternion.Inverse(m_refParImg.transform.rotation).eulerAngles;
                float fx = _RatatePointAroundCenter(pos, angle).x;
                float fy = _RatatePointAroundCenter(pos, angle).y * fDeltaTime;
            }
            #endregion

            #region Noise
            if (m_refParImg.m_NoiseModule.m_bEnabled)
            {
                float fNoise = 0f;
                if (m_refParImg.m_eSpace == ESimulation.Local)
                {
                    fNoise = m_refParImg.m_Noise.GetNoise(m_v2Position.x, m_v2Position.y);
                }
                else
                {
                    Vector2 v2Pos = m_v2Position + (Vector2)m_refParImg.transform.localPosition;
                    fNoise = m_refParImg.m_Noise.GetNoise(v2Pos.x, v2Pos.y);
                }

                m_v2NoiseVelocity = new Vector2(Mathf.Cos(fNoise * Mathf.PI), Mathf.Sin(fNoise * Mathf.PI)) * m_refParImg.m_fNoiseStrength;
            }
            #endregion

            #region Vortex
            if (m_refParImg.m_VortexModule.m_bEnabled)
            {
                float z = m_refParImg.m_curveVortexStrengh.Evaluate(fTimeProgress, m_fVortexLerp) * fDeltaTime * 100;

                m_v2ModifiedPosition = _RatatePointAroundCenter(m_v2ModifiedPosition, new(0, 0, z));
            }
            #endregion

            #region Attractor
            if (m_refParImg.m_tranAttractorTarget != null && m_refParImg.m_TargetModule.m_bEnabled)
            {
                Vector3 v3TargetPos = Vector3.zero;
                if (m_refParImg.m_tranAttractorTarget is RectTransform)
                {
                    v3TargetPos = m_Transform.InverseTransformPoint(m_refParImg.m_tranAttractorTarget.position);
                }
                else
                {
                    Vector3 v3ViewportPos = m_refParImg.WorldToViewportPoint(m_refParImg.m_tranAttractorTarget.position);
                    m_refParImg.m_eTargetMode = EAttractorType.Pivot;
                    Rect view = m_refParImg.m_tranCanvas.rect;
                    float fLocalX = v3ViewportPos.x.Remap(0.5f, 1.5f, 0, view.width);
                    float fLocalY = v3ViewportPos.y.Remap(0.5f, 1.5f, 0, view.height);
                    Vector3 v3LocalPosToCanvas = m_refParImg.m_tranCanvas.InverseTransformPoint(m_Transform.position);

                    if (m_refParImg.canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    {

                        fLocalX = fLocalX - v3LocalPosToCanvas.x + m_refParImg.m_tranCanvas.localPosition.x;
                        fLocalX = fLocalX / m_Transform.lossyScale.x * m_refParImg.m_tranCanvas.localScale.x;

                        fLocalY = fLocalY - v3LocalPosToCanvas.y + m_refParImg.m_tranCanvas.localPosition.y;
                        fLocalY = fLocalY / m_Transform.lossyScale.y * m_refParImg.m_tranCanvas.localScale.y;
                    }
                    else
                    {
                        fLocalX = fLocalX - v3LocalPosToCanvas.x;
                        fLocalX = fLocalX / m_Transform.lossyScale.x * m_refParImg.m_tranCanvas.localScale.x;

                        fLocalY = fLocalY - v3LocalPosToCanvas.y;
                        fLocalY = fLocalY / m_Transform.lossyScale.y * m_refParImg.m_tranCanvas.localScale.y;
                    }
                    v3TargetPos = new Vector3(fLocalX, fLocalY, 0);
                }
                if(m_refParImg.m_eTargetMode == EAttractorType.Pivot)
                {
                    float fLerp = m_refParImg.m_curveToTarget.Evaluate(fTimeProgress, m_fAttractorLerp);
                    m_v2Position = Vector3.LerpUnclamped(m_v2ModifiedPosition, v3TargetPos, fLerp);
                }
                else
                {

                }
            }
            else
            {
                m_v2Position = m_v2ModifiedPosition;
            }
            #endregion

            m_v2DeltaPos = m_v2Position - m_v2LastPoint;
            m_v2LastPoint = m_v2Position;

            Color c = m_refParImg.m_curveColorOverLifeTime.Evaluate(fTimeProgress, m_fColorLerp);
            float fNormalizedSpeed = m_v2DeltaPos.magnitude * (1f / Time.deltaTime) / 100f;
            float fSpeedColorRate = fNormalizedSpeed.Remap(m_refParImg.m_colorSpeedRange.m_fFrom, m_refParImg.m_colorSpeedRange.m_fTo, 0, 1);
            m_Color = m_StartColor * c * m_refParImg.m_curveColorBySpeed.Evaluate(fSpeedColorRate);

            Vector3 v3SizeOverTime = Vector3.one;
            if (m_refParImg.m_curveSizeOverLifeTime.m_bSeparated)
            {
                float x = m_refParImg.m_curveSizeOverLifeTime.m_curveX.Evaluate(fTimeProgress, m_fSizeLerp);
                float y = m_refParImg.m_curveSizeOverLifeTime.m_curveY.Evaluate(fTimeProgress, m_fSizeLerp);
                float z = m_refParImg.m_curveSizeOverLifeTime.m_curveZ.Evaluate(fTimeProgress, m_fSizeLerp);
                v3SizeOverTime = new Vector3(x, y, z);
            }
            else
            {
                float x = m_refParImg.m_curveSizeOverLifeTime.m_curveMain.Evaluate(fTimeProgress, m_fSizeLerp);
                float y = m_refParImg.m_curveSizeOverLifeTime.m_curveMain.Evaluate(fTimeProgress, m_fSizeLerp);
                float z = m_refParImg.m_curveSizeOverLifeTime.m_curveMain.Evaluate(fTimeProgress, m_fSizeLerp);
                v3SizeOverTime = new Vector3(x, y, z);
            }

            Vector3 v3SizeSpeed = Vector3.one;
            float fSpeedSizeRate = fNormalizedSpeed.Remap(m_refParImg.m_sizeSpeedRange.m_fFrom, m_refParImg.m_sizeSpeedRange.m_fTo, 0, 1);
            if (m_refParImg.m_curveSizeBySpeed.m_bSeparated)
            {
                float x = m_refParImg.m_curveSizeBySpeed.m_curveX.Evaluate(fSpeedSizeRate, m_fSizeLerp);
                float y = m_refParImg.m_curveSizeBySpeed.m_curveY.Evaluate(fSpeedSizeRate, m_fSizeLerp);
                float z = m_refParImg.m_curveSizeBySpeed.m_curveZ.Evaluate(fSpeedSizeRate, m_fSizeLerp);
                v3SizeSpeed = new Vector3(x, y, z);
            }
            else
            {
                float x = m_refParImg.m_curveSizeBySpeed.m_curveMain.Evaluate(fSpeedSizeRate, m_fSizeLerp);
                float y = m_refParImg.m_curveSizeBySpeed.m_curveMain.Evaluate(fSpeedSizeRate, m_fSizeLerp);
                float z = m_refParImg.m_curveSizeBySpeed.m_curveMain.Evaluate(fSpeedSizeRate, m_fSizeLerp);
                v3SizeSpeed = new Vector3(x, y, z);
            }
            m_v3Size = m_v3StartSize;

            float xs = m_v3Size.x * v3SizeOverTime.x * v3SizeSpeed.x;
            float ys = m_v3Size.y * v3SizeOverTime.y * v3SizeSpeed.y;
            float zs = m_v3Size.z * v3SizeOverTime.z * v3SizeSpeed.z;
            m_v3Size = new Vector3(xs, ys, zs);
        }

        public void Render(VertexHelper a_vh)
        {

        }

        private Vector3 _RatatePointAroundPivot(Vector3 a_v3Point, Vector3 a_v3Pivot, Vector3 a_v3Angles)
        {
            return Quaternion.Euler(a_v3Angles) * (a_v3Point - a_v3Pivot) + a_v3Pivot;
        }

        private Vector3 _RatatePointAroundCenter(Vector3 a_v3Point, Vector3 a_v3Angles)
        {
            return Quaternion.Euler(a_v3Angles) * a_v3Point;
        }


        public float TimeSineBron => m_fTime;
        public float LifeTime => m_fLifeTime;
    }

    struct SpriteSheet
    {
        public Vector2 v2Size;
        public Vector2 v2Pos;

        public SpriteSheet(Vector2 size, Vector2 pos)
        {
            v2Size = size;
            v2Pos = pos;
        }
    }

    struct TrialPoint
    {
        public Vector2 v2Pos;
        public float fTime;

        public TrialPoint(Vector2 pos, float time)
        {
            v2Pos = pos;
            fTime = time;
        }
    }
}
