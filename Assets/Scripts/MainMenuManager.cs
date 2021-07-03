using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

	//PRIVATE
	private Button btnStart;
	private Button btnQuit;

	// Use this for initialization
	void Start () {
		Camera.main.aspect = 16.0f / 9.0f;
		btnStart = GameObject.Find ("ButtonStart").GetComponent<Button>();
		btnQuit = GameObject.Find ("ButtonQuit").GetComponent<Button>();
		btnStart.onClick.AddListener (StartGame);
		btnQuit.onClick.AddListener (ExitGame);
	}

	void StartGame(){
		Debug.Log ("Start Game");
		Application.LoadLevel ("MainGame");
	}

	void ExitGame(){
		Debug.Log ("Exit Game");
		Application.Quit ();
	}
}
