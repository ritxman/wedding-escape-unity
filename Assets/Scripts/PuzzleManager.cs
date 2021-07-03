using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour {

	public Text debugPattern;
	public GameObject boardGameObject; //Prefab board gameobject
	public GameObject [] shapes = new GameObject[5]; //Prefab shapes

	private float x,y; //for generating boards
	private GameObject [,] boardInGame = new GameObject[7,7];
	private GameObject [,] shapeOnBoard = new GameObject[7,7];
	private GameObject [,] tempRespawnShape = new GameObject[7,7];
	private Transform shapeRespawner;
	private Transform shapeDiscard;
	private int[] xInputs = new int[6]; //for x coordinates input
	private int[] yInputs = new int[6]; //for y coordinates input
	private int[] xShapeDestroyed = new int[75];
	private int[] yShapeDestroyed = new int[75];
	private int[] xShapeOnFire = new int[75];
	private int[] yShapeOnFire = new int[75];
	private int[,] checkMoveArray = new int[7,7];
	private int counterShapeDestroyed = 0;
	private int counterSelectedShape;
	private int counterAnimateDestroyed = 0;
	private int counterShapeOnFire = 0;
	private int totalPossibleMove = 0;
	private int currentX; //current pointer
	private int currentY;
	private int multiplier;
	private bool[,] isDestroyed = new bool[7,7]; //for avoiding duplicate assign to destroying shape
	private bool[,] isBoardZeroAlready = new bool[7,7]; //check for fire shape creation
	private bool[,] isFireShape = new bool[7,7]; //for checking fire shape status
	private bool[,] isVisited = new bool[7,7];
	private bool[,] isVisitedMultiplier = new bool[7, 7];
	private bool isSelected; //user can select shape or not
	private bool isStartGame;
	private bool isGiveFireShape;
	private MainGameManager mgm;

	int [,] board = new int[7,7]; //for indexing shape type

	//INITIALIZE
	void Start(){
		debugPattern.text = "Ready?";
		clearInputsArray ();
		ResetAllTemp ();

		//reset isFireShape
		for(int i=0; i<7; i++){
			for(int j=0; j<7; j++){
				isFireShape[i,j] = false;
			}
		}
		isStartGame = false;
		isSelected = false;
		mgm = this.GetComponent<MainGameManager> ();
		mgm.SetTilesToGo (30);
		mgm.SetMoves (5);
		mgm.SetLevel (1);
		shapeRespawner = GameObject.Find ("Shape Respawner").GetComponent<Transform>();
		shapeDiscard = GameObject.Find ("Shape Discard").GetComponent<Transform>();
		StartCoroutine(AnimateGenerateBoards());
	}
	void clearInputsArray(){
		for(int i=0; i<5; i++){
			xInputs [i] = -1;
			yInputs [i] = -1;
		}
	}
	void clearDebugText(){
		debugPattern.text = "";
	}

	//FOR GENERATING BOARDS ANIMATION
	IEnumerator AnimateGenerateBoards(){
		x = -4.5f;
		y = 4.5f;
		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				boardInGame[i,j] = (GameObject)Instantiate (boardGameObject,new Vector3(x,y,0),Quaternion.identity);
				x += 1.65f;
				yield return new WaitForSeconds (0.03f);
			}
			y -= 1.8f;
			x = -4.5f;
		}	
		Invoke ("FillAllBoard",1f);
	}

	//FOR CHECK MOVE
	int CheckMove(){
		totalPossibleMove = 0;
		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				if (board [i, j] % 2 == 0)
					checkMoveArray [i, j] = board [i, j] - 1;
				else
					checkMoveArray [i, j] = board [i, j];
			}
		}
		//horizontal check
		for(int i=0; i<6; i++){
			for(int j=0; j<4; j++){
				if(checkMoveArray[i,j] == checkMoveArray[i,j+1] && checkMoveArray[i,j+1] == checkMoveArray[i,j+2]){
					totalPossibleMove++;
				}
			}
		}
		//vertical check
		for(int i=0; i<6; i++){
			for(int j=0; j<4; j++){
				if(checkMoveArray[j,i] == checkMoveArray[j+1,i] && checkMoveArray[j+1,i] == checkMoveArray[j+2,i]){
					totalPossibleMove++;
				}
			}
		}
		return totalPossibleMove;
	}

	//FILL ALL BOARD WITH SHAPE, USED WHEN THE BEGINNING OF THE GAME OR RESETTING THE BOARD IF THERE IS NO MOVE
	void FillAllBoard(){
		DestroyShapesWithTag ();
		int a;
		do{
			for(int i=0; i<6; i++){
				for(int j=0; j<6; j++){
					do{
						a = Random.Range (1,5);
						board[i,j] = a;
					}while(CheckPattern() == true);
				}
			}
		}while(CheckMove()<3);

		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				shapeOnBoard[i,j] = (GameObject)Instantiate (shapes[board[i,j]-1],new Vector3(0,0,0),Quaternion.identity);
				shapeOnBoard[i,j].transform.localPosition = new Vector3 (boardInGame[i,j].transform.localPosition.x,shapeRespawner.localPosition.y,0);
				shapeOnBoard[i,j].transform.localScale = new Vector3 (0.15f,0.15f,1);
				shapeOnBoard [i, j].name = i+","+j;
			}
		}
		//PrintBoard ();
		StartCoroutine (AnimateRespawnShape());

	}

	//ANIMATE SHAPE RESPAWNING FROM LEFT TO RIGHT
	IEnumerator AnimateRespawnShape(){
		for(int i=5; i>=0; i--){
			for(int j=0; j<6; j++){
				StartCoroutine (ShapeFallDownAnimation(shapeOnBoard[i,j].transform,boardInGame[i,j].transform));
				yield return new WaitForSeconds (0.04f);
			}
		}
		isStartGame = true;
		CanSelectShape ();
		debugPattern.text = "Go!";
		Invoke ("clearDebugText",1f);
	}

	//ANIMATE SHAPE FALL DOWN
	IEnumerator ShapeFallDownAnimation(Transform shape,Transform target){
		float speed = 0.05f;
		while(shape.transform.localPosition.y > target.localPosition.y){
			shape.transform.localPosition = new Vector3 (shape.transform.localPosition.x, shape.transform.localPosition.y - speed, shape.transform.localPosition.z);
			speed += 0.005f;
			yield return new WaitForSeconds (0.000001f);
		}
		shape.transform.localPosition = new Vector3 (shape.transform.localPosition.x, target.localPosition.y, shape.transform.localPosition.z);
	}

	//ANIMATE SHAPE ROTATION
	IEnumerator ShapeChangeAnimation(int x, int y, float start, float end){
		float width = start;
		while(width < end){
			width += 0.01f;
			shapeOnBoard[x,y].transform.localScale = new Vector3 (width,shapeOnBoard[x,y].transform.localScale.y,shapeOnBoard[x,y].transform.localScale.z);
			yield return new WaitForSeconds (0.0000001f);
		}
		shapeOnBoard[x,y].transform.localScale = new Vector3 (end,shapeOnBoard[x,y].transform.localScale.y,shapeOnBoard[x,y].transform.localScale.z);
	}

	public void ChangeShape(int x, int y, float start, float end){
		StartCoroutine (ShapeChangeAnimation(x,y,start,end));
	}
	public void SwapShapeType(int x, int y, int type){
		if (type % 2 == 1) {
			type += 1;
		} else {
			type -= 1;
		}
		board [x, y] = type;
		shapeOnBoard[x,y].GetComponent<SpriteRenderer>().sprite = shapes[type-1].GetComponent<SpriteRenderer>().sprite;
		ChangeShape (x,y, 0f, 0.15f);
	}
	bool CheckPattern(){
		//horizontal check
		int point = 0;
		bool flag = false;

		for(int i=0; i<6; i++){
			for(int j=0; j<5; j++){
				if (board [i, j] == board [i, j+1] && board [i, j+1] != 0) {
					point++;
				} else {
					point = 0;
				}
				if(point>=2){
					//PrintBoard ();
					flag = true;
				}
			}
			point = 0;
		}

		//vertical check
		for(int kolom=0; kolom<6; kolom++){
			for(int baris=0; baris<5; baris++){
				if (board [baris, kolom] == board [baris+1, kolom] && board [baris+1, kolom] != 0) {
					point++;
				} else {
					point = 0;
				}
				if(point>=2){
					//PrintBoard ();
					flag = true;
				}
			}
			point = 0;
		}

		return flag;
	}

	void CheckGiveFireShape(int x, int y, int type){
		if (isVisited [x, y]) {
			return;
		}
		isVisited [x, y] = true;
		if(isFireShape[x,y]){
			isGiveFireShape = false;
			return;
		}
		if (y - 1 >= 0) {
			if (board [x, y - 1] == type && isBoardZeroAlready [x, y - 1]) { //left
				CheckGiveFireShape (x, y - 1, type);
			}
		}
		if(y+1 <= 5){
			if(board[x,y+1] == type && isBoardZeroAlready[x,y+1]){ //right
				CheckGiveFireShape(x,y+1,type);
			}
		}
		if(x-1 >= 0){
			if(board[x-1, y] == type && isBoardZeroAlready[x-1,y]){ //up
				CheckGiveFireShape(x-1,y,type);
			}
		}
		if(x+1 <= 5){
			if(board[x+1, y] == type && isBoardZeroAlready[x+1,y]){ //down
				CheckGiveFireShape(x+1,y,type);
			}
		}
	}

	void CheckMultiplier(int x, int y, int type){
		if(isVisitedMultiplier[x,y]){
			return;
		}
		isVisitedMultiplier [x, y] = true;
		multiplier++;
		if (y - 1 >= 0) {
			if (board [x, y - 1] == type && isBoardZeroAlready [x, y - 1]) { //left
				CheckMultiplier (x, y - 1, type);
			}
		}
		if(y+1 <= 5){
			if(board[x,y+1] == type && isBoardZeroAlready[x,y+1]){ //right
				CheckMultiplier(x,y+1,type);
			}
		}
		if(x-1 >= 0){
			if(board[x-1, y] == type && isBoardZeroAlready[x-1,y]){ //up
				CheckMultiplier(x-1,y,type);
			}
		}
		if(x+1 <= 5){
			if(board[x+1, y] == type && isBoardZeroAlready[x+1,y]){ //down
				CheckMultiplier(x+1,y,type);
			}
		}
	}
	void CheckMatch(){
		int [] xTemp = new int[7];
		int [] yTemp = new int[7];
		int counterTemp = 0;
		int horizontalIndex = 0;
		int verticalIndex = 0;
		int point = 0;
		int x = -1;
		int y = -1;

		//horizontal check
		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				horizontalIndex = j+1;
				xTemp [counterTemp] = i;
				yTemp [counterTemp] = j;
				counterTemp++;
				for(int k=horizontalIndex; k<6; k++){
					if (board [i, j] == board [i, k]) {
						xTemp [counterTemp] = i;
						yTemp [counterTemp] = k;
						counterTemp++;
						point++;
					} else {
						j = k - 1;
						break;
					}
					if(k==5)j = k - 1;
				}
				if(point >= 2){
					for(int l=0; l<counterTemp; l++){
						isBoardZeroAlready[xTemp[l],yTemp[l]] = true;
						xShapeDestroyed[counterShapeDestroyed] = xTemp[l];
						yShapeDestroyed[counterShapeDestroyed] = yTemp[l];
						counterShapeDestroyed++;
					}
					if(point >= 3){
						if(shapeOnBoard[xTemp[0],yTemp[0]].GetComponent<ShapeController>().GetOnFire() == 0){
							isBoardZeroAlready[xTemp[0],yTemp[0]] = true;
							xShapeOnFire[counterShapeOnFire] = xTemp[0];
							yShapeOnFire[counterShapeOnFire] = yTemp[0];
							isFireShape [xTemp[0], yTemp[0]] = true;
							counterShapeOnFire++;
						}
					}
				}
				for(int l=0; l<counterTemp; l++){
					xTemp [l] = 0;
					yTemp [l] = 0;
				}
				counterTemp = 0;
				point = 0;
			}
		}

		//vertical check
		for(int kolom=0; kolom<6; kolom++){
			for(int baris=0; baris<6; baris++){
				verticalIndex = baris+1;
				xTemp [counterTemp] = baris;
				yTemp [counterTemp] = kolom;
				counterTemp++;
				for(int k=verticalIndex; k<6; k++){
					if (board [baris, kolom] == board [k, kolom]) {
						xTemp [counterTemp] = k;
						yTemp [counterTemp] = kolom;
						counterTemp++;
						point++;
					} else {
						baris = k - 1;
						break;
					}
					if(k==5)baris = k - 1;
				}
				if(point >= 2){
					x = -1;
					y = -1;
					for(int l=0; l<counterTemp; l++){
						if (!isBoardZeroAlready [xTemp [l], yTemp [l]]) {
							isBoardZeroAlready [xTemp [l], yTemp [l]] = true;
						} else {
							x = xTemp[l];
							y = yTemp[l];
						}
						xShapeDestroyed[counterShapeDestroyed] = xTemp[l];
						yShapeDestroyed[counterShapeDestroyed] = yTemp[l];
						counterShapeDestroyed++;
					}
					//isGiveFireShape = false;
					for(int i=0; i<6; i++){
						for(int j=0; j<6; j++){
							isVisited [i, j] = false;
						}
					}
					if (x != -1) {
						CheckGiveFireShape (x, y, board [x, y]);
					}
					
					if (point >= 3) {
						Debug.Log ("x:" + x + ",y" + y + " point: " + point + ", isgive: " + isGiveFireShape);
						if (isGiveFireShape) {
							if (x != -1) {
								isBoardZeroAlready [x, y] = true;
								xShapeOnFire [counterShapeOnFire] = x;
								yShapeOnFire [counterShapeOnFire] = y;
								isFireShape [x, y] = true;
								counterShapeOnFire++;
							} else {
								isBoardZeroAlready [xTemp [0], yTemp [0]] = true;
								xShapeOnFire [counterShapeOnFire] = xTemp [0];
								yShapeOnFire [counterShapeOnFire] = yTemp [0];
								isFireShape [xTemp [0], yTemp [0]] = true;
								counterShapeOnFire++;
							}
						}
					} else {
						if (isGiveFireShape) {
							if (x != -1) {
								isBoardZeroAlready[x,y] = true;
								xShapeOnFire [counterShapeOnFire] = x;
								yShapeOnFire [counterShapeOnFire] = y;
								isFireShape [x, y] = true;
								counterShapeOnFire++;
							}
						}
					}
				}
				for(int l=0; l<counterTemp; l++){
					xTemp [l] = 0;
					yTemp [l] = 0;
				}
				counterTemp = 0;
				point = 0;
			}
		}
		Invoke ("DestroyShapeOrCreateFireShape",0.4f);
	}

	//FOR RESPONDING INPUT
	public void CheckInput(int x, int y){
		if(isSelected == true){
			string name = x+","+y;
			if(counterSelectedShape == 0){ //first selection
				xInputs [counterSelectedShape] = currentX = x;
				yInputs [counterSelectedShape] = currentY = y;
				counterSelectedShape++;
				GameObject.Find (""+name).GetComponent<ShapeController>().SelectShape();
			}else{
				if(counterSelectedShape < 5){
					if((x == currentX-1 && y == currentY) ||
						(x == currentX+1 && y == currentY) ||
						(y == currentY-1 && x == currentX) ||
						(y == currentY+1 && x == currentX)
					){
						xInputs [counterSelectedShape] = currentX = x;
						yInputs [counterSelectedShape] = currentY = y;
						counterSelectedShape++;
						GameObject.Find (""+name).GetComponent<ShapeController>().SelectShape();
					}
				}
			}
		}
	}
	public void SwapSelectedShape(){
		for(int i=0; i<counterSelectedShape; i++){
			SwapShapeType(xInputs[i],yInputs[i],board[xInputs[i],yInputs[i]]);
		}
	}
	public void ResetInput(){
		clearInputsArray ();
		counterSelectedShape = 0;
		CanSelectShape ();
	}
	public void ResetAllTemp(){
		for(int i=0; i<75; i++){
			xShapeDestroyed [i] = -1;
			yShapeDestroyed [i] = -1;
			xShapeOnFire [i] = -1;
			yShapeOnFire [i] = -1;
		}
		for(int i=0; i<7; i++){
			for(int j=0; j<7; j++){
				isDestroyed [i,j] = false;
				isBoardZeroAlready [i, j] = false;
				isVisited [i, j] = false;
				isVisitedMultiplier [i, j] = false;
			}
		}
		isGiveFireShape = true;
		counterSelectedShape = 0;
		counterShapeDestroyed = 0;
		counterAnimateDestroyed = 0;
		counterShapeOnFire = 0;
		multiplier = 0;
	}
	public void Scoring(){
		SwapSelectedShape ();
		if (CheckPattern ()) {
			/*
			if(isSelected){
				mgm.SetMoves (mgm.GetMoves()-1);
			}
			*/
			isSelected = false;
			CheckMatch ();

		} else {
			isSelected = false;
			Invoke ("SwapSelectedShape",0.4f);
			Invoke ("ResetInput",0.65f);
		}
	}
	public void CheckDestroyedFireShape(){
		int type = 0;
		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				type = shapeOnBoard[i,j].GetComponent<ShapeController>().GetOnFire();
				if(type == 2){
					int multiplier = shapeOnBoard [i, j].GetComponent<ShapeController> ().GetMultiplier ();
					StartCoroutine (GiveMultiplierAnimation(i,j,multiplier));
					break;
				}
			}
			if(type == 2){
				break;
			}
		}
		if(type != 2){
			ResetInput ();
		}
	}

	IEnumerator GiveMultiplierAnimation(int baris, int kolom, int multiplier){
		int [] xTemp = new int[10];
		int [] yTemp = new int[10];
		int temp = 0;
		xTemp[0] = baris-1;
		yTemp[0] = kolom-1;
		xTemp[1] = baris-1;
		yTemp[1] = kolom;
		xTemp[2] = baris-1;
		yTemp[2] = kolom+1;

		xTemp[3] = baris;
		yTemp[3] = kolom-1;
		xTemp[4] = baris;
		yTemp[4] = kolom+1;

		xTemp[5] = baris+1;
		yTemp[5] = kolom-1;
		xTemp[6] = baris+1;
		yTemp[6] = kolom;
		xTemp[7] = baris+1;
		yTemp[7] = kolom+1;
		while(multiplier > 0){
			do{
				temp = Random.Range (0,8);
			}while(xTemp[temp] < 0 || xTemp[temp] > 5 || yTemp[temp] < 0 || yTemp[temp] > 5);
			int mult = shapeOnBoard [xTemp [temp], yTemp [temp]].GetComponent<ShapeController> ().GetMultiplier ();
			shapeOnBoard [xTemp [temp], yTemp [temp]].GetComponent<ShapeController> ().SetMultiplier (mult+1);
			multiplier--;
			shapeOnBoard [baris,kolom].GetComponent<ShapeController> ().SetMultiplier (multiplier);
			if(multiplier == 0){
				counterShapeDestroyed = 1;
				isFireShape [baris, kolom] = false;
				DestroySpesificShape (baris,kolom,0);
			}
			yield return new WaitForSeconds (0.5f);
		}
	}

	public void DestroySpesificShape(int x, int y, int isCreateFire){
		if(isCreateFire == 0){
			board [x, y] = 0;
		}
		StartCoroutine (AnimateDestroyShape (x, y, isCreateFire));
	}
	public void DestroyShapeOrCreateFireShape(){ //destroy the selected shape and create fire shape
		int isCreateFire = 0;

		//create multiplier
		int onFireType = 0;
		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				multiplier = 0;
				onFireType = shapeOnBoard [i, j].GetComponent<ShapeController> ().GetOnFire ();
				if(isFireShape[i,j] && onFireType == 0){
					CheckMultiplier (i,j,board[i,j]);
					shapeOnBoard [i, j].GetComponent<ShapeController> ().SetMultiplier (multiplier-3);
				}
			}
		}
		//destroy shape or create fire
		for(int i=0; i<counterShapeDestroyed; i++){
			isCreateFire = 0;
			for(int j=0; j<counterShapeOnFire; j++){
				if(xShapeDestroyed[i] == xShapeOnFire[j] && yShapeDestroyed[i] == yShapeOnFire[j]){
					isCreateFire = 1;
					DestroySpesificShape (xShapeOnFire [j], yShapeOnFire [j], isCreateFire);
					break;
				}
			}
			if (!isFireShape [xShapeDestroyed [i], yShapeDestroyed [i]]) {
				isCreateFire = 0;
			} else {
				isCreateFire = 2;
			}
			DestroySpesificShape (xShapeDestroyed [i], yShapeDestroyed [i], isCreateFire);
		}

	}
	IEnumerator AnimateDestroyShape(int x, int y, int isFire){ //shape destroyed animation
		if(!isDestroyed[x,y]){
			isDestroyed [x, y] = true;
			if (isFire == 0) {
				while (shapeOnBoard [x, y].transform.localScale.x > 0) {
					shapeOnBoard [x, y].transform.localScale = new Vector3 (shapeOnBoard [x, y].transform.localScale.x - 0.01f, shapeOnBoard [x, y].transform.localScale.y - 0.01f, shapeOnBoard [x, y].transform.localScale.z);
					yield return new WaitForSeconds (0.0005f);
				}
				shapeOnBoard [x, y].transform.localScale = new Vector3 (0, 0, 0);
				shapeOnBoard [x, y].tag = shapeOnBoard [x, y].name = "Destroyed";
			} else if (isFire == 1) {
				shapeOnBoard [x, y].GetComponent<ShapeController> ().SetOnFire (1);
			} else {
				shapeOnBoard [x, y].GetComponent<ShapeController> ().SetOnFire (2);
			}
		}
		counterAnimateDestroyed++;
		if(counterAnimateDestroyed == counterShapeDestroyed){
			Invoke ("ShapeFall",0.1f);
		}
	}

	public void ShapeFall(){
		for(int baris=5; baris>=0 ; baris--){
			for(int kolom=0; kolom<6; kolom++){
				if(board[baris,kolom] == 0){ //the box is empty, move down shape above that coloumn
					for(int baris2=baris-1; baris2>=0; baris2--){
						if(board[baris2,kolom] != 0){
							board[baris,kolom] = board[baris2, kolom];
							board [baris2, kolom] = 0;
							isFireShape [baris, kolom] = isFireShape[baris2,kolom];
							isFireShape [baris2, kolom] = false;
							shapeOnBoard [baris2, kolom].name = baris+","+kolom;
							StartCoroutine (ShapeFallDownAnimation (shapeOnBoard[baris2,kolom].transform, boardInGame [baris, kolom].transform));
							break;
						}
					}
				}
			}
		}
		StartCoroutine (RespawnShape());
	}
	IEnumerator RespawnShape(){
		int height = 0;
		bool isHeightIncreased = false;
		for(int i=5; i>=0; i--){
			for(int j=0; j<6; j++){
				if (board [i, j] == 0) {
					int a = Random.Range (1, 5);
					board [i, j] = a;
					tempRespawnShape [i, j] = (GameObject)Instantiate (shapes [a - 1], new Vector3 (0, 0, 0), Quaternion.identity);
					tempRespawnShape [i, j].transform.localPosition = new Vector3 (boardInGame [i, j].transform.localPosition.x, shapeRespawner.localPosition.y+height, 0);
					tempRespawnShape [i, j].transform.localScale = new Vector3 (0.15f, 0.15f, 1);
					tempRespawnShape [i, j].name = i + "," + j;
					StartCoroutine (ShapeFallDownAnimation (tempRespawnShape [i, j].transform, boardInGame [i, j].transform));
					if(!isHeightIncreased){
						height++;
					}
					isHeightIncreased = true;
				}
				shapeOnBoard [i, j] = GameObject.Find (i+","+j);
				shapeOnBoard [i, j].name = i+","+j;
				yield return new WaitForSeconds (0.0005f);
			}
			isHeightIncreased = false;
		}
		Invoke ("CheckNextMatch",0.3f);

	}
	public void CheckNextMatch(){
		DestroyShapesWithTag ();
		//PrintBoard ();
		ResetAllTemp ();
		if (CheckPattern ()) {
			Invoke ("Scoring",0.1f);
		} else {
			if(CheckMove() > 0){
				Invoke ("CheckDestroyedFireShape",0.5f);
			}else{
				Invoke("ResetAllShapes",0.5f);
			}
		}
	}

	//DESTRY SHAPE WITH Destroyed TAG
	public void DestroyShapesWithTag(){
		GameObject [] temp = new GameObject[100];
		temp = GameObject.FindGameObjectsWithTag ("Destroyed");
		int length = temp.Length;
		for(int i=0; i<length; i++){
			Destroy (temp[i]); //destroy all shape with destroyed tag
		}
	}
	public void ResetAllShapes(){
		debugPattern.text = "No moves!\nResetting board";
		StartCoroutine (DiscardAllShapes());
		ResetAllTemp ();
		Invoke ("FillAllBoard",4f);
	}

	IEnumerator DiscardAllShapes(){
		for(int i=5; i>=0; i--){
			for(int j=0; j<6; j++){
				shapeOnBoard [i, j].tag = shapeOnBoard [i, j].name = "Destroyed";
				StartCoroutine (ShapeFallDownAnimation (shapeOnBoard [i, j].transform, shapeDiscard));
				yield return new WaitForSeconds (0.005f);
			}
		}
		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				isFireShape [i, j] = false;
			}
		}
	}

	public void PrintBoard(){
		debugPattern.text = "";
		for(int i=0; i<6; i++){
			for(int j=0; j<6; j++){
				debugPattern.text += board[i,j]+" ";
			}
			debugPattern.text += "\n";
		}
	}
	public void CanSelectShape(){
		if(mgm.GetMoves() == 0){
			mgm.ShowPopUp ("GameOver");
		}else{
			isSelected = true;
		}
	}

	void Update(){
		if(Input.GetMouseButtonUp(0) && isSelected == true){
			Scoring ();
		}
	}
}