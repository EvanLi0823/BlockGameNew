using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Libs
{
    //	[RequireComponent(typeof(Button))]
    public class UIButtonBase : MonoBehaviour
    {
        public delegate void OnButtonClick (GameObject go);

        public OnButtonClick OnClick, OnSelected;
        public RectTransform m_Transform;
        protected Vector3 OriginScale;

        void Awake ()
        {
            if (m_Transform == null) {
                m_Transform = transform as RectTransform;
            }
            
        }

        void Start ()
        {
            UGUIEventListener.Get (gameObject).onPointIn = ButtonDownEffect;
            UGUIEventListener.Get (gameObject).onPointOut = ButtonUpEffect;
			OriginScale = m_Transform.localScale;
        }


        public virtual void ButtonHover (GameObject go, bool isValue)
        {
        }

        public virtual void ButtonDownEffect (GameObject go)
        {
        }

        public virtual void ButtonUpEffect (GameObject go)
        {
        }

        public virtual void ButtonEnterEffect ()
        {
        }

        public virtual void ButtonExitEffect ()
        {
        }

        public virtual void ButtonSelectEffect ()
        {
        }

        public virtual void ButtonDeselectEffect ()
        {
        }
    }
}
