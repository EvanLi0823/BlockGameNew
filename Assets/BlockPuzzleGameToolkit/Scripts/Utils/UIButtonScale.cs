using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Libs{
    public class UIButtonScale : UIButtonBase {
        public float scaleXOnPressed = 1.1f;
        public float scaleYOnPressed = 1.1f;
        public float scaleDurationOnPressed = 0.3f;
        public float scaleDurationOnReleased = 0.3f;

        public float scaleOnSelected = 1.05f;
        public float scaleDurationOnSelected = 0.1f;
        public float scaleDurationOnDeselected = 0.1f;

		public bool IsBlock = false;
		public override void ButtonHover(GameObject go,bool isValue){
//			if (isValue) {
//				m_Transform.DOScale(0.9f, 0.3f);
//			} else {
//				m_Transform.DOScale(1.0f, 0.3f);
//			}
		}
		public override void ButtonDownEffect(GameObject go)
		{ 
            m_Transform.DOScaleX(this.OriginScale.x * scaleXOnPressed, scaleDurationOnPressed);
            m_Transform.DOScaleY(this.OriginScale.y * scaleYOnPressed, scaleDurationOnPressed);
		}
		
		public override void ButtonUpEffect(GameObject go)
		{
			if (!IsBlock) {
				m_Transform.DOScaleX (this.OriginScale.x, scaleDurationOnReleased);
				m_Transform.DOScaleY (this.OriginScale.y, scaleDurationOnReleased);
			}
		}
		
		public override void ButtonSelectEffect()
		{
			if (!IsBlock) {
				m_Transform.DOScale (scaleOnSelected, scaleDurationOnSelected);
			}
		}
		
		public override void ButtonDeselectEffect()
		{
			if (!IsBlock) {
				m_Transform.DOScale (1.0f, scaleDurationOnDeselected);
			}
		}

		public void CalledDown(){
			if (!IsBlock) {
				m_Transform.DOScaleX (scaleXOnPressed, scaleDurationOnPressed);
				m_Transform.DOScaleY (scaleYOnPressed, scaleDurationOnPressed);
			}
		}

		public void CalledUp(){
			if (!IsBlock) {
				m_Transform.DOScaleX (1, scaleDurationOnReleased);
				m_Transform.DOScaleY (1, scaleDurationOnReleased);
			}
		}

	}
}
