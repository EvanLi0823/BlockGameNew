using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIProgressBar : MonoBehaviour {

	private Image progressbar;
	void Awake(){
		progressbar = transform.GetComponent<Image> ();
	}
	public virtual void SetProgress(double progress){
		progressbar.fillAmount = Mathf.Clamp01 ((float)progress);
	}
}
