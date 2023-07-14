using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using Random = UnityEngine.Random;

namespace ParticleImage
{
    [AddComponentMenu("UI/Particle Image/Particle Image")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CParticleImage : MaskableGraphic
    {
        [SerializeField]
        public ESimulation m_eSpace = ESimulation.Local;
        [SerializeField]
        public ETimeScale m_eTimeScale = ETimeScale.Normal;

        [SerializeField]
        public ESpreadType m_eSpreadType = ESpreadType.Random;
        [SerializeField]
        public float m_fSpreadLoop = 1;

        [SerializeField]
        public Module m_EmitterModule = new Module(false);
        [SerializeField]
        public Transform m_tranEmitterConstraint;


        [SerializeField]
        public EEmitterShape m_eEmitterShape = EEmitterShape.Circle;
        [SerializeField]
        public float m_fEmitterRadius = 50;
        [SerializeField]
        public float m_fEmitterWidth = 100;
        [SerializeField]
        public float m_fEmitterHeight = 100;
        [SerializeField]
        public float m_fEmitterAngle = 45;
        [SerializeField]
        public float m_fEmitterLen = 100f;
        [SerializeField]
        public bool m_bEmitterFitRect;
        [SerializeField]
        public bool m_bEmitOnSurface = true;
        [SerializeField]
        public float m_fEmitThickness;

        [SerializeField]
        public bool m_bLoop = true;
        [SerializeField]
        public float m_fDuration = 5f;
        [SerializeField]
        public float m_fStartDelay = 0;
        [SerializeField]
        public EPlayMode m_ePlayMode = EPlayMode.OnAwake;

        [SerializeField]
        public float m_fRate = 50;
        [SerializeField]
        public float m_fRateOverLifeTime = 0;
        [SerializeField]
        public float m_fRateOverDistance = 0;


        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveLifeTime = new ParticleSystem.MinMaxCurve(1f);


        [SerializeField]
        public ParticleSystem.MinMaxGradient m_curveStartColor = new ParticleSystem.MinMaxGradient(Color.white);
        [SerializeField]
        public ParticleSystem.MinMaxGradient m_curveColorOverLifeTime = new ParticleSystem.MinMaxGradient(new Gradient());
        [SerializeField]
        public ParticleSystem.MinMaxGradient m_curveColorBySpeed = new ParticleSystem.MinMaxGradient(new Gradient());
        [SerializeField]
        public SpeedRange m_colorSpeedRange = new SpeedRange(0, 1);

        [SerializeField]
        public SeparatedMinMaxCurve m_curveStartSize = new SeparatedMinMaxCurve(40f);
        [SerializeField]
        public SeparatedMinMaxCurve m_curveSizeOverLifeTime = new SeparatedMinMaxCurve(new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) }));
        [SerializeField]
        public SeparatedMinMaxCurve m_curveSizeBySpeed = new SeparatedMinMaxCurve(new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) }));
        [SerializeField]
        public SpeedRange m_sizeSpeedRange = new SpeedRange(0, 1);

        [SerializeField]
        public SeparatedMinMaxCurve m_curveStartRotation = new SeparatedMinMaxCurve(0f);
        [SerializeField]
        public SeparatedMinMaxCurve m_curveRotationOverLifeTime = new SeparatedMinMaxCurve(new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) }));
        [SerializeField]
        public SeparatedMinMaxCurve m_curveRotationBySpeed = new SeparatedMinMaxCurve(new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) }));
        [SerializeField]
        public SpeedRange m_sizeRotationRange = new SpeedRange(0, 1);

        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveStartSpeed = new ParticleSystem.MinMaxCurve(2f);
        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveSpeedOverTime = new ParticleSystem.MinMaxCurve(1);


        [SerializeField]
        public bool m_bAlignToDirection;


        [SerializeField]
        public Module m_TargetModule = new Module(false);
        [SerializeField]
        public Transform m_tranAttractorTarget;
        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveToTarget = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new[] { new Keyframe(0, 0), new Keyframe(1, 1) }));
        [SerializeField]
        public EAttractorType m_eTargetMode = EAttractorType.Pivot;


        [SerializeField]
        public Module m_NoiseModule = new Module(false);
        public Noise m_Noise = new Noise();
        [SerializeField]
        public int m_nNoiseOctaves = 1;
        [SerializeField]
        public float m_fNoiseFrequency = 1f;
        [SerializeField]
        public float m_fNoiseStrength = 1f;


        [SerializeField]
        public Module m_GravityModule = new Module(false);
        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveGravity = new ParticleSystem.MinMaxCurve(-9.81f);

        [SerializeField]
        public Module m_VelocityModule = new Module(false);
        [SerializeField]
        public ESimulation m_eVelocitySpace;
        [SerializeField]
        public SeparatedMinMaxCurve m_VelocityOverLifeTime = new SeparatedMinMaxCurve(0f, true, false);


        [SerializeField]
        public Module m_VortexModule = new Module(false);
        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveVortexStrengh;

        [SerializeField]
        public Module m_SheetModule = new Module(false);
        [SerializeField]
        public Vector2Int m_v2SheetTile = Vector2Int.one;
        [SerializeField]
        public ESheetType m_eSheetType = ESheetType.FPS;
        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveSheetStartFrame = new ParticleSystem.MinMaxCurve(0);
        [SerializeField]
        public int m_nSheetFPS = 25;
        [SerializeField]
        public SpeedRange m_SheetSpeedRange = new SpeedRange(0, 1);
        [SerializeField]
        public ParticleSystem.MinMaxCurve m_curveSheetFrameOverTime = new ParticleSystem.MinMaxCurve(0f);
        [SerializeField]
        public int m_nSheetCycles = 1;

        private UnityEvent m_eveOnStart = new UnityEvent();
        private UnityEvent m_eveFirstParFinish = new UnityEvent();
        private UnityEvent m_eveLastParFinish = new UnityEvent();
        private UnityEvent m_eveParFinish = new UnityEvent();
        private UnityEvent m_eveOnStop = new UnityEvent();


        private bool m_bMoudleEmitterFoldout;
        private bool m_bMoudleParFoldout;
        private bool m_bMoudleMovementFoldout;
        private bool m_bMoudleEventFoldout;

        private CParticleImage m_main;
        private CParticleImage[] m_arrChild;
        public RectTransform m_tranCanvas;

        private List<CParticle> m_listPar = new List<CParticle>();
        private List<Burst> m_listBurst = new List<Burst>();

        private float m_fTime;
        private float m_fLoopTimer;

        private float m_t;
        private float m_t2;
        private float m_fBurstTimer;

        private Vector2 m_v2Position;
        private Vector3 m_v3LastPosition;
        private Vector3 m_v3DeltaPosition;

        private bool m_bEmitting;
        private bool m_bPlaying;
        private bool m_bStoped;
        private bool m_bPaused;

        private bool m_bFirstParFinished;

        private int m_nOrderPerSec;
        private int m_nOrderOverLife;
        private int m_nOrderOverDistance;

        protected override void Awake()
        {
            _InitNoise();
            Clear();

            if (m_ePlayMode == EPlayMode.OnAwake && Application.isPlaying)
            {
                Play();
            }
        }

        public void OnEnable()
        {
            if (_IsMain())
            {
                m_arrChild = _GetChildren();
            }

            m_main = _GetMain();
            m_main.m_arrChild = m_main._GetChildren();

            m_v3LastPosition = transform.position;
            if (canvas != null)
            {
                m_tranCanvas = canvas.gameObject.GetComponent<RectTransform>();
            }

            if (m_ePlayMode == EPlayMode.OnEnable && Application.isPlaying)
            {
                Stop(true);
                Clear();
                Play();
            }

            RecalculateMasking();
            RecalculateClipping();
            SetAllDirty();
        }


        public void Play()
        {
            _GetMain()._DoPlay();
        }

        void Update()
        {
            _Animate();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < m_listPar.Count; i++)
            {
                CParticle par = m_listPar[i];
            }
        }

        public void Stop(bool a_bClear)
        {
            _GetMain()._DoStop(a_bClear);
        }

        public void Clear()
        {
            _GetMain()._DoClear();
        }

        private void _DoStop(bool a_bClear)
        {
            _MainTraversingChild((parImg) => parImg._DoStop(a_bClear));
            _ResetBurst();

            m_nOrderPerSec = 0;
            m_nOrderOverLife = 0;
            m_nOrderOverDistance = 0;

            if (a_bClear)
            {
                m_bStoped = true;
                m_bPlaying = false;
                Clear();
            }

            m_bEmitting = false;
            if (m_bPaused)
            {
                m_bPaused = false;
                m_bStoped = true;
                m_bPlaying = false;
                Clear();
            }

            _ResetBurst();
            m_bFirstParFinished = false;
            _SetVerticesAndMaterialDirty();
        }


        private void _DoPlay()
        {
            _MainTraversingChild((parImg) => parImg._DoPlay());
            m_eveOnStart.Invoke();

            _ResetBurst();

            m_fTime = 0;
            m_fBurstTimer = 0;
            m_bEmitting = true;
            m_bPlaying = true;
            m_bPaused = false;
            m_bStoped = false;
        }

        private void _DoClear()
        {
            _MainTraversingChild((parImg) => parImg._DoClear());
            _ResetBurst();
            m_listPar.Clear();
            m_fTime = 0;
            m_fBurstTimer = 0;
            _SetVerticesAndMaterialDirty();
        }

        private void _ResetBurst()
        {
            for (int i = 0; i < m_listBurst.Count; i++)
            {
                m_listBurst[i].m_bUsed = false;
            }
        }

        private void _MainTraversingChild(Action<CParticleImage> a_fn)
        {
            if (!_IsMain() || m_arrChild == null || a_fn == null)
            {
                return;
            }
            foreach (var parImg in m_arrChild)
            {
                a_fn.Invoke(parImg);
            }
        }

        private bool _IsMain()
        {
            return _GetMain() == this;
        }

        private CParticleImage _GetMain()
        {
            if (m_main == null)
            {
                m_main = __GetMain();
            }
            return m_main;
        }

        private CParticleImage __GetMain()
        {
            if (transform.parent)
            {
                if (transform.parent.TryGetComponent<CParticleImage>(out CParticleImage p))
                {
                    return p.__GetMain();
                }
            }
            return this;
        }

        private CParticleImage[] _GetChildren()
        {
            if (transform.childCount <= 0)
            {
                return null;
            }

            var ch = GetComponentsInChildren<CParticleImage>().Where(t => t != this);
            if (ch.Any())
            {
                return ch.ToArray();
            }
            return null;
        }

        private bool _CanStop()
        {
            if (m_arrChild == null)
            {
                return true;
            }

            return m_arrChild.All(t => t.m_bEmitting == false && t.m_listPar.Count <= 0);
        }

        private void _SetVerticesAndMaterialDirty()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }
        private void _InitNoise()
        {
            m_Noise.SetNoiseType(Noise.NoiseType.OpenSimplex2S);
            m_Noise.SetFrequency(m_fNoiseFrequency * 0.01f);
            m_Noise.SetFractalOctaves(m_nNoiseOctaves);
        }

        private Camera _GetCamera()
        {
            return Camera.main;
        }

        public Vector3 WorldToViewportPoint(Vector3 a_pos)
        {
            Vector3 pos = _GetCamera().WorldToViewportPoint(a_pos);
            return pos;
        }

        private Vector3 _RotateOnAngle(Vector3 a_pos, float a_angle)
        {
            return Quaternion.Euler(new Vector3(0, 0, a_angle)) * a_pos;
        }

        private Vector3 _RotateOnAngle(float a_angle)
        {
            float fRad = a_angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Sin(fRad), Mathf.Cos(fRad), 0);
            return pos;
        }

        private Vector2 _GetPointOnRect(float a_Angle, float a_w, float a_h)
        {
            float fRadians = a_Angle * Mathf.Deg2Rad;
            float fSin = Mathf.Sin(a_Angle);
            float fCos = Mathf.Cos(a_Angle);

            float fDy = fSin > 0 ? a_h * 0.5f : a_h * -0.5f;
            float fDx = fCos > 0 ? a_w * 0.5f : a_w * -0.5f;

            if (Mathf.Abs(fDx * fSin) < Mathf.Abs(fDy * fCos))
            {
                fDy = (fDx * fSin) / fCos;
            }
            else
            {
                fDx = (fDy * fCos) / fSin;
            }

            // Return the point as a Vector2 object
            return new Vector2(fDx, fDy);
        }

        #region 粒子发射逻辑
        private void _Animate()
        {
            if (m_bPlaying)
            {
                m_v3DeltaPosition = transform.position - m_v3LastPosition;

                if (m_tranEmitterConstraint != null && m_EmitterModule.m_bEnabled)
                {
                    if (m_tranEmitterConstraint is RectTransform)
                    {
                        transform.position = m_tranEmitterConstraint.position;
                    }
                    else
                    {
                        Vector3 v3CanPos;
                        Vector3 v3ViewPos = _GetCamera().WorldToViewportPoint(m_tranEmitterConstraint.position);

                        v3CanPos = new Vector3(v3ViewPos.x.Remap(0.5f, 1.5f, 0f, m_tranCanvas.rect.width), v3ViewPos.y.Remap(0.5f, 1.5f, 0, m_tranCanvas.rect.height), 0);

                        v3CanPos = m_tranCanvas.transform.TransformPoint(v3CanPos);

                        v3CanPos = transform.parent.InverseTransformPoint(v3CanPos);

                        transform.localPosition = v3CanPos;
                    }
                }

                float fDeltaTime = m_eTimeScale == ETimeScale.Normal ? Time.deltaTime : Time.unscaledDeltaTime;
                if (_IsMain())
                {
                    m_fTime += fDeltaTime;
                }
                else
                {
                    m_fTime = _GetMain().m_fTime;
                }
                m_fLoopTimer += fDeltaTime;
                m_fBurstTimer += fDeltaTime;

                _SetVerticesAndMaterialDirty();
            }

            if (m_bEmitting)
            {
                float fDeltaTime = m_eTimeScale == ETimeScale.Normal ? Time.deltaTime : Time.unscaledDeltaTime;
                if (m_fRate > 0)
                {
                    if ((m_bLoop || m_fTime < (m_fDuration + m_fStartDelay)) && m_fTime > m_fStartDelay)
                    {
                        float fDur = 1f / m_fRate;
                        m_t += fDeltaTime;
                        while (m_t >= m_fDuration)
                        {
                            m_t -= fDur;
                            m_nOrderPerSec++;
                            _GenterateParticle(m_nOrderPerSec, EGenerateParType.RatePerSed, null);
                        }
                    }
                }

                if (m_fRateOverLifeTime > 0)
                {
                    if ((m_bLoop || m_fTime < (m_fDuration + m_fStartDelay)) && m_fTime > m_fStartDelay)
                    {
                        float fDur = m_fDuration / m_fRateOverLifeTime;
                        m_t2 += fDeltaTime;
                        while (m_t2 >= m_fDuration)
                        {
                            m_t2 -= fDur;
                            m_nOrderOverLife++;
                            _GenterateParticle(m_nOrderOverLife, EGenerateParType.RateOverLife, null);
                        }
                    }
                }

                if (m_fRateOverDistance > 0)
                {
                    if (m_v3DeltaPosition.magnitude > 1.0f / m_fRateOverDistance)
                    {
                        m_nOrderOverDistance++;
                        _GenterateParticle(m_nOrderOverDistance, EGenerateParType.RateOverDistance, null);
                        m_v3LastPosition = transform.position;
                    }
                }

                if (m_listBurst != null)
                {
                    for (int i = 0; i < m_listBurst.Count; i++)
                    {
                        Burst b = m_listBurst[i];
                        if (m_fBurstTimer > b.m_fTime + m_fStartDelay && b.m_bUsed == false)
                        {
                            for (int j = 0; j < b.m_nCount; j++)
                            {
                                _GenterateParticle(j, EGenerateParType.Burst, b);
                            }
                            b.m_bUsed = true;
                        }
                    }
                }

                if (m_bLoop && m_fBurstTimer >= m_fDuration)
                {
                    m_fBurstTimer = 0;
                    _ResetBurst();
                }

                if (m_fTime >= m_fDuration + m_fStartDelay && !m_bLoop)
                {
                    m_bEmitting = false;
                }

                if (m_bLoop && m_fLoopTimer >= m_fDuration + m_fStartDelay)
                {
                    m_fLoopTimer = 0;
                    m_nOrderPerSec = 0;
                    m_nOrderOverLife = 0;
                    m_nOrderOverDistance = 0;
                }
            }

            if (m_bPlaying && !m_bEmitting && _IsMain() && m_listPar.Count <= 0)
            {
                if (_CanStop())
                {
                    m_eveOnStop.Invoke();
                    Stop(true);
                }
            }
        }

        private void _GenterateParticle(int a_nOrder, EGenerateParType a_eGenerateType, Burst a_burst)
        {
            float fAngle = 0;
            if (a_eGenerateType == EGenerateParType.Burst)
            {
                fAngle = a_nOrder * (360f / m_listBurst.Count) * m_fSpreadLoop;
            }
            else if (a_eGenerateType == EGenerateParType.RatePerSed)
            {
                fAngle = a_nOrder * (360f / m_fRate) / m_fDuration * m_fSpreadLoop;
            }
            else if (a_eGenerateType == EGenerateParType.RateOverLife)
            {
                fAngle = a_nOrder * (360f / m_fRateOverLifeTime) * m_fSpreadLoop;
            }
            else if (a_eGenerateType == EGenerateParType.RateOverDistance)
            {
                fAngle = a_nOrder * (360f / m_fRateOverDistance) * m_fSpreadLoop;
            }

            Vector2 v2Pos = Vector2.zero;
            switch (m_eEmitterShape)
            {
                case EEmitterShape.Point:
                    v2Pos = Vector2.zero;
                    break;
                case EEmitterShape.Circle:
                    if (m_bEmitOnSurface)
                    {
                        if (m_eSpreadType == ESpreadType.Random)
                        {
                            v2Pos = (UnityEngine.Random.insideUnitCircle * m_fEmitterRadius);
                        }
                        else
                        {
                            v2Pos = _RotateOnAngle(new Vector3(0, 1.0f, 0), fAngle) * m_fEmitterRadius;
                        }
                    }
                    else
                    {
                        if (m_eSpreadType == ESpreadType.Random)
                        {
                            Vector2 v2R = UnityEngine.Random.insideUnitCircle.normalized;
                            v2Pos = Vector2.Lerp(v2R * m_fEmitterRadius, v2R * (m_fEmitterRadius - m_fEmitThickness), UnityEngine.Random.value);
                        }
                        else
                        {
                            v2Pos = _RotateOnAngle(new Vector3(0, 1.0f, 0), fAngle) * UnityEngine.Random.Range(m_fEmitterRadius - m_fEmitThickness, m_fEmitterRadius);
                        }
                    }
                    break;
                case EEmitterShape.Rectangle:
                    if (m_bEmitOnSurface)
                    {
                        if (m_eSpreadType == ESpreadType.Random)
                        {
                            float fx = UnityEngine.Random.Range(m_fEmitterWidth * -0.5f, m_fEmitterWidth * 0.5f);
                            float fy = UnityEngine.Random.Range(m_fEmitterHeight * -0.5f, m_fEmitterHeight * 0.5f);
                            v2Pos = new Vector2(fx, fy);
                        }
                        else
                        {
                            v2Pos = _GetPointOnRect(fAngle, m_fEmitterWidth, m_fEmitterHeight);
                        }
                    }
                    else
                    {
                        float fTempAngle = fAngle;
                        if (m_eSpreadType == ESpreadType.Random)
                        {
                            fTempAngle = UnityEngine.Random.Range(0, 360);
                        }
                        Vector2 v2Max = _GetPointOnRect(fTempAngle, m_fEmitterWidth, m_fEmitterHeight);
                        Vector2 v2Min = _GetPointOnRect(fTempAngle, m_fEmitterWidth - m_fEmitThickness, m_fEmitterHeight - m_fEmitThickness);
                        v2Pos = Vector2.Lerp(v2Min, v2Max, UnityEngine.Random.value);
                    }
                    break;
                case EEmitterShape.Line:
                    if (m_eSpreadType == ESpreadType.Random)
                    {
                        float fx = UnityEngine.Random.Range(m_fEmitterLen * -0.5f, m_fEmitterLen * 0.5f);
                        v2Pos = new Vector2(fx, 0);
                    }
                    else
                    {
                        float fx = Mathf.Repeat(fAngle, 361).Remap(0, 360, m_fEmitterLen * -0.5f, m_fEmitterLen * 0.5f);
                        v2Pos = new Vector2(fx, 0);
                    }
                    break;
                case EEmitterShape.Directional:
                    v2Pos = Vector2.zero;
                    break;
            }

            if (m_eSpace == ESimulation.World)
            {
                v2Pos = Quaternion.Euler(transform.eulerAngles) * v2Pos;
            }

            Vector2 v2Velocity = Vector2.zero;
            float fStartVelocitySpeed = m_curveStartSpeed.Evaluate(UnityEngine.Random.value, UnityEngine.Random.value);
            switch (m_eEmitterShape)
            {
                case EEmitterShape.Point:
                    if (m_eSpreadType == ESpreadType.Random)
                    {
                        v2Velocity = Random.insideUnitCircle * fStartVelocitySpeed;
                    }
                    else
                    {
                        v2Velocity = _RotateOnAngle(Vector3.up, fAngle) * fStartVelocitySpeed;
                    }
                    break;
                case EEmitterShape.Circle:
                case EEmitterShape.Rectangle:
                    v2Velocity = v2Pos.normalized * fStartVelocitySpeed;
                    break;
                case EEmitterShape.Line:
                    v2Velocity = (m_eSpace == ESimulation.Local ? transform.up : Vector3.up) * fStartVelocitySpeed;
                    break;
                case EEmitterShape.Directional:
                    float fTempAngle = 0;
                    if (m_eSpace == ESimulation.World)
                    {
                        fTempAngle = Mathf.Repeat(fAngle, 361).Remap(0, 360, m_fEmitterAngle * -0.5f, m_fEmitterAngle * 0.5f);
                        fTempAngle = fTempAngle - transform.eulerAngles.z;
                    }
                    else
                    {
                        fTempAngle = Mathf.Repeat(fAngle, 361).Remap(0, 360, m_fEmitterAngle * -0.5f, m_fEmitterAngle * 0.5f);
                    }
                    v2Velocity = _RotateOnAngle(fAngle) * fStartVelocitySpeed;
                    break;
            }

            Vector3 v3Rotation = m_curveStartRotation.m_bSeparated
                                ? new Vector3(
                                    m_curveStartRotation.m_curveX.Evaluate(Random.value, Random.value)
                                    , m_curveStartRotation.m_curveY.Evaluate(Random.value, Random.value)
                                    , m_curveStartRotation.m_curveZ.Evaluate(Random.value, Random.value)
                                    )
                                : new Vector3(0, 0, m_curveStartRotation.m_curveMain.Evaluate(Random.value, Random.value));
            Color color = m_curveStartColor.Evaluate(Random.value, Random.value);
            float fSizeLerp = Random.value;
            Vector3 v3Size = m_curveStartSize.m_bSeparated
                            ? new Vector3(
                                m_curveStartSize.m_curveX.Evaluate(Random.value, Random.value)
                                , m_curveStartSize.m_curveY.Evaluate(Random.value, Random.value)
                                , m_curveStartSize.m_curveZ.Evaluate(Random.value, Random.value)
                                )
                            : new Vector3(
                                m_curveStartSize.m_curveX.Evaluate(fSizeLerp, fSizeLerp)
                                , m_curveStartSize.m_curveY.Evaluate(fSizeLerp, fSizeLerp)
                                , m_curveStartSize.m_curveZ.Evaluate(fSizeLerp, fSizeLerp)
                                );
            float fLifeTime = m_curveLifeTime.Evaluate(Random.value, Random.value);

            CParticle par = new CParticle(this, v2Pos, v3Rotation, v2Velocity, color, v3Size, fLifeTime);
        }
        #endregion

        public Material material
        {
            get
            {
                return m_Material;
            }
            set
            {
                if (m_Material == value) return;
                m_Material = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        [SerializeField]
        public Texture m_Texture;
        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value) return;
                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        public override Texture mainTexture
        {
            get
            {
                return m_Texture == null ? s_WhiteTexture : m_Texture;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            foreach (var par in m_listPar)
            {
                par.Animate();
                par.Render(vh);
            }
        }
    }

    [Serializable]
    public class Burst
    {
        public float m_fTime;
        public int m_nCount;
        public bool m_bUsed;

        public Burst(float time, int a_nCount)
        {
            m_fTime = time;
            m_nCount = a_nCount;
            m_bUsed = false;
        }
    }

    [Serializable]
    public struct SpeedRange
    {
        public float m_fFrom;
        public float m_fTo;

        public SpeedRange(float from, float to)
        {
            m_fFrom = from;
            m_fTo = to;
        }
    }

    [Serializable]

    public struct Module
    {
        public bool m_bEnabled;

        public Module(bool a_bEnable)
        {
            m_bEnabled = a_bEnable;
        }
    }

    [Serializable]
    public struct SeparatedMinMaxCurve
    {
        [SerializeField]
        private bool m_bSeparable;
        public bool m_bSeparated;

        public ParticleSystem.MinMaxCurve m_curveMain;
        public ParticleSystem.MinMaxCurve m_curveX;
        public ParticleSystem.MinMaxCurve m_curveY;
        public ParticleSystem.MinMaxCurve m_curveZ;

        public SeparatedMinMaxCurve(float a_fStartVal, bool a_bSeparated = false, bool a_bSeparable = true)
        {
            m_curveMain = new ParticleSystem.MinMaxCurve(a_fStartVal);
            m_curveX = new ParticleSystem.MinMaxCurve(a_fStartVal);
            m_curveY = new ParticleSystem.MinMaxCurve(a_fStartVal);
            m_curveZ = new ParticleSystem.MinMaxCurve(a_fStartVal);
            m_bSeparated = a_bSeparated;
            m_bSeparable = a_bSeparable;
        }

        public SeparatedMinMaxCurve(AnimationCurve a_startVal, bool a_bSeparated = false, bool a_bSeparable = false)
        {
            m_curveMain = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(a_startVal.keys));
            m_curveX = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(a_startVal.keys));
            m_curveY = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(a_startVal.keys));
            m_curveZ = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(a_startVal.keys));

            m_bSeparable = a_bSeparable;
            m_bSeparated = a_bSeparated;
        }

    }

    public static class Extensions
    {
        public static float Remap(this float val, float from1, float to1, float from2, float to2)
        {
            float v = (val - from1) / (to1 - from1) * (to2 - from2) + from2;
            if (float.IsNaN(v) || float.IsInfinity(v))
            {
                return 0f;
            }
            return v;
        }
    }
    #region 枚举
    public enum EGenerateParType
    {
        Burst, RatePerSed, RateOverLife, RateOverDistance
    }
    public enum EEmitterShape
    {
        Point, Circle, Rectangle, Line, Directional
    }

    public enum ESpreadType
    {
        Random, Uniform
    }

    public enum ESimulation
    {
        Local, World
    }

    public enum EAttractorType
    {
        Pivot, Surface
    }

    public enum EPlayMode
    {
        None, OnEnable, OnAwake
    }

    public enum ESheetType
    {
        Lifetime, Speed, FPS
    }

    public enum ETimeScale
    {
        Unscaled, Normal
    }

    #endregion
}
