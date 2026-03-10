using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class UIText : MonoBehaviour {

	private TextMeshProUGUI TxtNumber;
	private Text TxtNumber2;
	void Awake()
	{
		Init();
	}

	public void Init()
	{
		if (TxtNumber==null) {
			TxtNumber = transform.GetComponent<TextMeshProUGUI> ();
		}
		if(TxtNumber2==null){
			TxtNumber2 = transform.GetComponent<Text> ();
		}
	}
	public void SetText(string text)
	{
		Init();
		if (TxtNumber != null) 
		{
			TxtNumber.text = text; 
		} 
		else if(TxtNumber2!=null)
		{
			TxtNumber2.text = text; 
		}
	}
	public void SetText(string text,Color color){
		Init();
		if (TxtNumber != null) 
		{
			TxtNumber.text = text;
			TxtNumber.color = color;
		} 
		else if(TxtNumber2!=null)
		{
			TxtNumber2.text = text;
			TxtNumber2.color = color;
		}
	}

	public string Text
	{
		get
		{
			if (TxtNumber != null)  return TxtNumber.text;
			if (TxtNumber2 != null) return TxtNumber2.text;
			return string.Empty;
		}
	}

	public void SetAlpha(float alpha)
	{
		Init();
		if (TxtNumber != null)  TxtNumber.alpha = alpha;
		if (TxtNumber2 != null)
		{
			Color color = TxtNumber2.color;
			TxtNumber2.color = new Color(color.r,color.g,color.b,alpha);
		}
	}
}
