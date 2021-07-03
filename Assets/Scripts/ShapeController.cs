using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeController : MonoBehaviour {

	//PUBLIC
	public Text multiplierText;
	public GameObject fire;
	public Animator fireAnimator;

	//PRIVATE
	private Color cSelected;
	private Color cNormal;
	private bool isClicked = false;
	private PuzzleManager pm;
	private MainGameManager mgm;
	private int multiplier;
	private int onFireType;

	void Start(){
		SetMultiplier (0);
		onFireType = 0;
		mgm = GameObject.Find ("Manager").GetComponent<MainGameManager>();
		pm = GameObject.Find ("Manager").GetComponent<PuzzleManager>();
		cSelected = GetComponent<SpriteRenderer>().color;
		cNormal = GetComponent<SpriteRenderer>().color;
		cSelected.a = 0.5f;
		cNormal.a = 1f;
	}

	void OnMouseEnter(){
		if(isClicked == true && !mgm.GetIsPaused()){
			pm.CheckInput (((int)this.name[0])-48, ((int)this.name[2])-48);
		}
	}
	void OnMouseDown(){
		if(!mgm.GetIsPaused())
			pm.CheckInput (((int)this.name[0])-48, ((int)this.name[2])-48);
	}
	void Update(){
		if(Input.GetMouseButtonDown(0)){
			isClicked = true;
		}else if(Input.GetMouseButtonUp(0)){
			isClicked = false;
			UnselectShape ();
		}
	}

	public void SelectShape(){
		GetComponent<SpriteRenderer> ().color = cSelected;
	}
	public void UnselectShape(){
		GetComponent<SpriteRenderer> ().color = cNormal;
	}
	public void SetMultiplier(int multiplier){
		this.multiplier = multiplier;
		if(this.multiplier <= 0){
			this.multiplierText.text = "";
		}else{
			this.multiplierText.text = "+"+this.multiplier;
		}
	}
	public int GetMultiplier(){
		return this.multiplier;
	}
	public void SetOnFire(int onFireType){ //1: fire, 2s: fire destroyed
		this.onFireType = onFireType;
		fire.SetActive (true);
		if(this.onFireType == 2){
			fireAnimator.SetTrigger ("isDestroyed");
		}
	}
	public int GetOnFire(){
		return this.onFireType;
	}
}
