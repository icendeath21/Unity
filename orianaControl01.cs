using UnityEngine;
using System.Collections;
/*
                    |"|
                   _|_|_
                   (o o)
+--------------ooO--(_)--Ooo--------------+
|
| File Name: orianaControl01
| Description:Allows movement and manipulation of Oriana
| 
| 
| 
|
| Last Revision: 3/21/2015
| Revisor: Tom Edwards
| notes: 1) Run, jump and walk are all functional. Still working on controller.
| 		 2) Climb works with animation. Use still image for "idle on ladder" and climb motion while moving.
| 
|
+-----------------------------------------+

+------------------Index------------------+
|0.0 - variables
|1.0 - start
|2.0 - update
|	2.1.1 lookup check
|	2.2.1 crouch check
|	2.3.1 jump check
|	2.4.1 climbing check
|	2.5.1 sidle check
|	2.6.1 death check
|	2.7.1 hit check
|		2.7.1 heavy hit check
|		2.7.2 light hit check
|		2.7.3 knockdown check
|	2.8.1 horizontal rope check
|		2.8.1 horizontal rope grab check
|		2.8.2 horizontal rope move check
|	2.9.1 pull check
|	2.10.1 push check
|	2.11.1 swing check
|	2.12.1 grab ledge check
|	2.13.1 grounded check
|	2.14.1 Charcter movement
|		2.14.2 fall check
|		2.14.3 abrupt stop check
|		2.14.4 sidle curve
|		2.14.5 pull curve
|		2.14.6 push curve
|		2.14.7 sneak move
|		2.14.8 basic movement curve
|		2.14.9 fun idle load
|	2.15.1 flip call
|3.0 - FixedUpdate
|	
|4.0 - Flip
|5.0 - 
|6.0 - 
|7.0 - 
|8.0 - 
|9.0 - 
|10.0 - 
+-----------------------------------------+

*/

public class orianaControl01 : MonoBehaviour {
	//==================================================================
	//	0.0.1 - variables
	//==================================================================
	public float maxSpeed = 7f;//note- max speed of character
	bool facingRight = false;//note- determines if facinf left or right
	Animator anim;//note- calls animator and sets using variable
	bool grounded = false;
	public Transform groundCheck;//note- where the ground should be
	float groundRadius = 0.2f;
	public LayerMask whatIsGround;//note- what is considered ground as far as layers are considered
	public float jumpForce = 275f;
	public bool canClimb = false;
	public bool climbMove = false;
	public int spawnPoint;
	public bool crouch = false;
	public bool lookUp = false;
	public bool lookDown = false;
	public bool sidleMove = false;
	public bool isSidle = false;
	public bool canSidle = false;
	public bool canGrab = false;
	public bool isSwing = false;
	public bool ledge = false;
	public bool canPush = false;
	public bool canPull = false;
	public float index=0f;
	public float indexPull=.5f;
	public float indexPush=.2f;
	public float stopIndex=0f;
	public bool charStop = false;
	public float indexFunIdle = 0f;
	public float sidleLock = 0f;
	//==================================================================
	//	1.0.1 - start
	//==================================================================
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		DontDestroyOnLoad(gameObject);
	}

	//==================================================================
	// 2.0.1 Update
	//==================================================================
	void Update(){

		//2.1.1 lookUp check
		//initial check for looking down
		if (Input.GetAxis("Vertical")<0 && grounded && canClimb == false){ 
			anim.SetBool("lookDown",true);
			lookDown = true;
		}//goes back to normal look forward
		else if (Input.GetAxis("Vertical")>0 && grounded && canClimb == false){ 
			anim.SetBool("lookUp",true);
			lookUp = true;
		}//goes back to normal look forward
		else if(Input.GetAxis("Vertical")==0){ 
			anim.SetBool("lookUp",false);
			lookUp = false;
			anim.SetBool("lookDown",false);
			lookDown = false;
		}

		//2.2.1 crouch check
		if(Input.GetAxis("Crouch")>0){ 
				crouch = true;
				anim.SetBool("crouch", true);
		}
		else if(Input.GetAxis("Crouch")>=0){ 
			crouch = false;
			anim.SetBool("crouch", false);
		}

		//2.3.1 jump check
		//note- did it hit any colliders true- on ground false = not on ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
		//note- sets value of grounded
		anim.SetBool("ground",grounded);

		if(grounded == true && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && isSidle == false)//note- initiates jump
		{
			crouch = false;
			anim.SetBool("crouch", false);
			anim.SetBool("ground", false);
			rigidbody2D.gravityScale = 1;
			rigidbody2D.AddForce(new Vector2(0,jumpForce));//note- handles jump speed
		}

		//2.4.1 climbing check
		if(canClimb == true && Input.GetAxis("Vertical") > 0){
			rigidbody2D.gravityScale = 0;
			gameObject.transform.Translate(Vector2.up * Time.deltaTime * 4);
			anim.SetBool("climbMove", true);
			anim.SetBool("canClimb", true);
			}
		else if(canClimb == true && Input.GetAxis("Vertical") < 0){
			rigidbody2D.gravityScale = 0;
			gameObject.transform.Translate(Vector2.up * Time.deltaTime *-4);
			anim.SetBool("climbMove", true);
			anim.SetBool("canClimb", true);
		}
		else if(canClimb == true && Input.GetAxis("Vertical") == 0){
			rigidbody2D.gravityScale = 0;
			anim.SetBool("climbMove", false);
			rigidbody2D.velocity = Vector2.zero;
		}

		// jump off ladder
		if(!canClimb && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))) {
			rigidbody2D.gravityScale = 1;
			anim.SetBool("climbMove", false);
			anim.SetBool("canClimb", false);
		}
		//2.5.1 sidle check
		if((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1)) && canSidle == true && grounded == true){

			if(isSidle == false){
				//player activates sidle
				anim.SetBool("isSidle", true);
				isSidle = true;
				index = 0f;
			}
			else if(isSidle == true){
				// player turns off sidle
				anim.SetBool("isSidle", false);
				isSidle = false;
				index = 0f;
			}

		}
		if(canSidle == false){
			// leave sidle area
			anim.SetBool("isSidle", false);
			isSidle = false;
			index = 0f;
		}

		//2.6.1 death check
		if(Input.GetKey(KeyCode.X))//sets oriana to dead animation - change when health added
			anim.SetBool("dead", true);
		else if(Input.GetKeyUp(KeyCode.X))
			anim.SetBool("dead", false);

		//2.7.1 hit check
		//2.7.1 heavy hit check
		if(Input.GetKey(KeyCode.H))//sets oriana to heavyHit animation - change when health added
			anim.SetBool("heavyHit", true);
		else if(Input.GetKeyUp(KeyCode.H))
			anim.SetBool("heavyHit", false);

		//2.7.2 light hit check
		if(Input.GetKey(KeyCode.L))//sets oriana to lightHit animation - change when health added
			anim.SetBool("lightHit", true);
		else if(Input.GetKeyUp(KeyCode.L))
			anim.SetBool("lightHit", false);

		//2.7.3 knockdown check
		if(Input.GetKey(KeyCode.K))//sets oriana to lightHit animation - change when health added
			anim.SetBool("knockDown", true);
		else if(Input.GetKeyUp(KeyCode.K))
			anim.SetBool("knockDown", false);

		//2.8.1 horizontal rope grab check
		if(canGrab == true){
			anim.SetBool("canGrab", true);
		}
		else if(canGrab == false)
		{
			anim.SetBool("canGrab", false);
		}
		//2.8.2 horizontal rope move check
		if(canGrab == true && Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
			anim.SetBool("grabMove",true);
		else if(canGrab == true && Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
			anim.SetBool("grabMove", false);

		//2.9.1 pull check
		if(canPull){

			anim.SetBool("canPull",true);}
		else{

			indexPull= .5f;
			anim.SetBool("canPull",false);
		}
		//2.10.1 push check
		if(canPush){

			anim.SetBool("canPush",true);}
		else{
			anim.SetBool("canPush",false);

			indexPush = .2f;
		}
		//2.11.1 swing check
		if(isSwing == true)
			anim.SetBool("swing", true);
		else if(isSwing == false)
			anim.SetBool("swing", false);


		//2.12.1 grab ledge check
		if(ledge == true)
			anim.SetBool("grabLedge", true);
		else if(ledge == false)
			anim.SetBool("grabLedge", false);

		//2.13.1 grounded check
		
		if(grounded == false){
			anim.SetBool("canClimb",canClimb);
			anim.SetBool("crouch",crouch);
			anim.SetBool("canSidle",canSidle);
			rigidbody2D.gravityScale = 1;
		}
		
		anim.SetFloat("vSpeed",rigidbody2D.velocity.y);
		
		//2.14.1 Charcter movement
		float move = Input.GetAxis("Horizontal");
		
		//2.14.2 fall check
		if(rigidbody2D.velocity.y < 0 && grounded == false)
			anim.SetBool("isFalling", true);
		else
			anim.SetBool("isFalling", false);
		
		
		if(grounded == true){
			if(crouch == true)
				maxSpeed = 4f;
			else if(isSidle == true)
				maxSpeed = 2f*index;
			else if(canPull == true)
				maxSpeed = 2f*indexPull;
			else if(canPush == true)
				maxSpeed = 2f*indexPush;
			else
				maxSpeed = 7f;
		}
		else if(grounded == false){
			maxSpeed = 5.5f;
			
		}
		
		//2.14.3 abrupt stop check
		anim.SetBool("charStop",false);
		if(move == 0f && Input.GetAxis("Stopturn") >0)
		{
			charStop = true;
			anim.SetBool("charStop",true);
		}
		if(move == 0f && Input.GetAxis("Stopturn") < 0)
		{
			charStop = true;
			anim.SetBool("charStop",true);
		}	
		
		//2.14.4 sidle curve
		if(isSidle && move != 0){
			index +=Time.deltaTime;
			if(index >= 1.175f)
				index = .0f;
			//tells animator current speed
			anim.SetFloat("speed", Mathf.Abs(move)*index);//note- allows interaction between animator and script for movement
			//moves sprite character
			rigidbody2D.velocity = new Vector2(((move*maxSpeed)*index)*1.2f, rigidbody2D.velocity.y);//note- allows movement
			
		}
		else if(isSidle && move == 0){
			index = .0f;
			anim.SetFloat("speed", 0f);
		}
		else if(canPull && move != 0){//2.14.5 pull curve
			indexPull +=Time.deltaTime;
			if(indexPull >= 1.44f)
				indexPull = .5f;
			//tells animator current speed
			anim.SetFloat("speed", Mathf.Abs(move)*(indexPull*.92f));//note- allows interaction between animator and script for movement
			//moves sprite character
			rigidbody2D.velocity = new Vector2(((move)*maxSpeed)*(indexPull*.92f), rigidbody2D.velocity.y);//note- allows movement
		}
		else if(canPull && move == 0){
			indexPull = .5f;
			anim.SetFloat("speed", 0f);
		}
		else if(canPush && move != 0){//2.14.6 push curve
			indexPush +=Time.deltaTime;
			//tells animator current speed
			anim.SetFloat("speed", Mathf.Abs(move)*indexPush);//note- allows interaction between animator and script for movement
			if(indexPush >= 1.17f){
				//moves sprite character
				rigidbody2D.velocity = new Vector2(((move)*maxSpeed)*(indexPush*1.25f), rigidbody2D.velocity.y);//note- allows movement
				indexPush = .2f;
			}
		}
		else if(canPush && move == 0){
			indexPush = .2f;
			anim.SetFloat("speed", 0f);
		}
		else{//2.14.8 basic movement curve
			
			if(lookUp ==false || lookDown == false){
				//tells animator current speed
				anim.SetFloat("speed", Mathf.Abs(move));//note- allows interaction between animator and script for movement
				//moves sprite character
				rigidbody2D.velocity = new Vector2(move*(maxSpeed), rigidbody2D.velocity.y);//note- allows movement
			}
		}
		//2.14.9 fun idle load
		if(move == 0 && grounded){
			indexFunIdle += Time.deltaTime;
		}
		else if(move != 0 || grounded == false){
			indexFunIdle = 0;
		}
		
		if(indexFunIdle > 10){
			indexFunIdle = 0;
		}
		anim.SetFloat("funIdle",indexFunIdle);
		
		//2.15.1 flip call
		//note- calls flip function to flip animation
		if (move>0 && !facingRight){
			if(canPull == false && canPush == false){//doesnt allow image to flip while sidleing
				if(charStop == false)
					Flip();
				else if(charStop == true){
					stopIndex += Time.deltaTime;
					if(stopIndex >= .4f){
						Flip();
						charStop = false;
						stopIndex = 0f;
					}
				}
			}
		}
		else if (move <0 && facingRight){
			if(canPull == false && canPush == false){//doesnt allow image to flip while sidleing
				if(charStop == false)
					Flip();
				else if(charStop == true){
					stopIndex += Time.deltaTime;
					if(stopIndex >=.4f){
						Flip();
						charStop = false;
						stopIndex = 0f;
					}
				}
			}
		}

	}

	//==================================================================
	// 3.0.1 FixedUpdate
	//==================================================================
	//note- Update is called once per frame
	void FixedUpdate () {

	}
	//==================================================================
	//	4.0.1 - Flip
	//==================================================================
	//note- flips animation based on direction the character is directed
	void Flip(){
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	void OnLevelWasLoaded(int level) {
		if(spawnPoint != null) {
			GameObject spawnLocation = GameObject.Find("Door" + spawnPoint);
			if(spawnLocation != null) {
				Vector3 temp = spawnLocation.transform.position;
				temp.z = transform.position.z;
				gameObject.transform.position = temp;
			}
		}
	}
}
