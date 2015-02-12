using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AdjustManager : MonoBehaviour {

	public BlueToothManager manager;
	public Text text;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		text.text = "ADJ_Roll:" + manager.DefaultRoll +"\nADJ_Pitch:"+manager.DefaultPitch;
	}
}
