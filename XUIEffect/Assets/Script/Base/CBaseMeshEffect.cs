
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace UIEffect
{
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public abstract class CBaseMeshEffect : UIBehaviour, IMeshModifier
    {
        RectTransform m_rectTransform;
        Graphic m_graphic;
        CGraphicConnector m_connector;


        public Graphic graphic
        {
            get
            {
                return m_graphic ? m_graphic : m_graphic = GetComponent<Graphic>();
            }
        }
        protected CGraphicConnector _connector
        {
            get
            {
                return m_connector ?? (m_connector = CGraphicConnector.Find(m_graphic));
            }
        }
        protected RectTransform _rectTransform
        {
            get
            {
                return m_rectTransform ? m_rectTransform : m_rectTransform = GetComponent<RectTransform>();
            }
        }


        public virtual void ModifyMesh(Mesh mesh)
        {
        }

        public virtual void ModifyMesh(VertexHelper verts)
        {

        }

        public virtual void MondifyMesh(VertexHelper verts, Graphic graphic)
        {

        }

        protected virtual void _SetVerticesDirty()
        {
            _connector.SetVerticesDirty(graphic);
        }

        protected override void OnEnable()
        {
            _connector.OnEnable(graphic);
            _SetVerticesDirty();
        }

        protected override void OnDisable()
        {
            _connector.OnDisable(graphic);
            _SetVerticesDirty();
        }

        protected virtual void _SetEffectParamsDirty()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            _SetVerticesDirty();
        }

        protected virtual void _OnDidApplyAnimationProperties()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            _SetEffectParamsDirty();
        }

#if UNITY_EDITOR

        protected override void Reset()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            _SetVerticesDirty();
        }

        protected override void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            _SetEffectParamsDirty();
        }
#endif
    }
}


