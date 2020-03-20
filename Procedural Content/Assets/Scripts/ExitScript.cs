using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExitScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void QuitApp()
    {
        //Application.quit();

#if UNITY_EDITOR
        Debug.Log("Exit");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
