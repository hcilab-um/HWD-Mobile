using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		Window.Init();
	
		Vector3 pos = new Vector3(0, 1, 0);
		Quaternion rot = Quaternion.Euler(30, 45, 0);
		GameObject window = Window.NewWindow(pos, rot, Window.WindowSize.Large, Color.blue, 1, true, 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
