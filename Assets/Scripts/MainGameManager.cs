using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameManager : MonoBehaviour {

	//PUBLIC
	public GameObject popUp;
	public GameObject backToMainMenuConfirmationWindow;
	public GameObject gameOverWindow;

	public GameObject bombUnavailable;
	public GameObject mineUnavailable;
	public GameObject magnetUnavailable;
	public GameObject flipAllUnavailable;

	public GameObject bombReadyImage;
	public GameObject mineReadyImage;
	public GameObject magnetReadyImage;
	public GameObject flipAllReadyImage;

	public GameObject bombCostParent;
	public GameObject mineCostParent;
	public GameObject magnetCostParent;
	public GameObject flipAllCostParent;

	public Text bombCostText;
	public Text mineCostText;
	public Text magnetCostText;
	public Text flipAllCostText;

	public Text tilesToGoText;
	public Text movesText;
	public Text levelText;

	//PRIVATE
	private bool isBackToMainMenu = true; //Give some delay while closing main menu window
	private bool isPaused = false;
	private int tilesToGo = 0;
	private int moves = 0;
	private int level = 1;

	private int bombCost = 0;
	private int mineCost = 0;
	private int magnetCost = 0;
	private int flipAllCost = 0;

	public void SetIsPaused(bool isPaused){
		this.isPaused = isPaused;
	}
	public bool GetIsPaused(){
		return this.isPaused;
	}
	void Start(){
		SetBombCost (5);
		SetMineCost (7);
		SetMagnetCost (10);
		SetFlipAllCost (12);
		SetBombAvailable (false);
		SetMineAvailable (false);
		SetMagnetAvailable (false);
		SetFlipAllAvailable (false);
		Camera.main.aspect = 16.0f / 9.0f;
	}
	//SETTER AND GETTER
	public void SetTilesToGo(int tilesToGo){
		this.tilesToGo = tilesToGo;
		tilesToGoText.text = "Tiles To Go:\n"+GetTilesToGo();
	}
	public int GetTilesToGo(){
		return this.tilesToGo;
	}
	public void SetMoves(int moves){
		this.moves = moves;
		movesText.text = "Moves:\n"+GetMoves();
	}
	public int GetMoves(){
		return this.moves;
	}
	public void SetLevel(int level){
		this.level = level;
		levelText.text = "Level:\n"+GetLevel();
	}
	public int GetLevel(){
		return this.level;
	}
	public void ShowPopUp(string name){ //Show specific pop up by parameter name
		if(name.Equals("BackToMainMenu")){
			if (isBackToMainMenu) { //for avoiding brute force click at the back to main menu button
				isBackToMainMenu = false;
				popUp.SetActive (true);
				BackToMainMenu ();
			}
		}else if(name.Equals("GameOver")){
			popUp.SetActive (true);
			GameOver ();
		}
	}
	public void HidePopUp(){ //Reset all pop up
		Invoke("CanClickBackToMainMenu",1f);
		SetIsPaused (false);
		backToMainMenuConfirmationWindow.SetActive (false);
		gameOverWindow.SetActive (false);
		popUp.SetActive (false);
	}

	void CanClickBackToMainMenu(){
		isBackToMainMenu = true;
	}
	void BackToMainMenu(){
		SetIsPaused (true);
		backToMainMenuConfirmationWindow.SetActive (true);
		backToMainMenuConfirmationWindow.GetComponent<Animator> ().SetTrigger("IsShowPopUpClip");
	}
	void GameOver(){
		SetIsPaused (true);
		gameOverWindow.SetActive (true);
		gameOverWindow.GetComponent<Animator> ().SetTrigger("IsShowPopUpClip");
	}
		
	public void OptionChoose(string name){
		if(name.Equals("YesBackToMainMenu")){
			YesBackToMainMenu ();
		}else if(name.Equals("NoBackToMainMenu")){
			NoBackToMainMenu ();
		}else if(name.Equals("YesTryAgain")){
			YesTryAgain ();
		}else if(name.Equals("NoTryAgain")){
			NoTryAgain ();
		}
	}

	public void YesBackToMainMenu(){
		Application.LoadLevel ("MainMenu");
	}
	public void NoBackToMainMenu(){
		HidePopUp ();
	}
	public void YesTryAgain(){
		Application.LoadLevel ("MainGame");
	}
	public void NoTryAgain(){
		Application.LoadLevel ("MainMenu");
	}
	public void SetBombAvailable(bool flag){
		bombUnavailable.SetActive (!flag);
		bombReadyImage.SetActive (flag);
		bombCostParent.SetActive (!flag);
	}
	public void SetMineAvailable(bool flag){
		mineUnavailable.SetActive (!flag);
		mineReadyImage.SetActive (flag);
		mineCostParent.SetActive (!flag);
	}
	public void SetMagnetAvailable(bool flag){
		magnetUnavailable.SetActive (!flag);
		magnetReadyImage.SetActive (flag);
		magnetCostParent.SetActive (!flag);
	}
	public void SetFlipAllAvailable(bool flag){
		flipAllUnavailable.SetActive (!flag);
		flipAllReadyImage.SetActive (flag);
		flipAllCostParent.SetActive (!flag);
	}
	public void SetBombCost(int cost){
		bombCostText.text = ""+cost;
	}
	public int GetBombCost(){
		return this.bombCost;
	}
	public void SetMineCost(int cost){
		mineCostText.text = ""+cost;
	}
	public int GetMineCost(){
		return this.mineCost;
	}
	public void SetMagnetCost(int cost){
		magnetCostText.text = ""+cost;
	}
	public int GetMagnetCost(){
		return this.magnetCost;
	}
	public void SetFlipAllCost(int cost){
		flipAllCostText.text = ""+cost;
	}
	public int GetFlipAllCost(){
		return this.flipAllCost;
	}
}
