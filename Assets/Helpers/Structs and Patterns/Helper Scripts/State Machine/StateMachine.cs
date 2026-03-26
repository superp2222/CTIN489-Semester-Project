/*
 * FINITE STATE MACHINE
 * ====================
 * 
 * A generic state machine for managing object behavior through discrete states.
 * Handles state transitions, tracks state history, and supports global states.
 * 
 * KEY FEATURES:
 * -------------
 * - currentState: The active state (Execute() called every Update())
 * - prevState: Previous state (accessible via RevertToPreviousState())
 * - globalState: Optional state that executes alongside currentState
 * - allowChangeToSameState: Toggle to prevent or allow transitioning to current state
 * 
 * USAGE EXAMPLE:
 * --------------
 * Complete player controller with Idle, Walking, and Jumping states:
 * 
 * public class PlayerController : MonoBehaviour
 * {
 *     public StateMachine<PlayerController> stateMachine;
 *     public Rigidbody rb;
 *     
 *     void Start()
 *     {
 *         stateMachine = new StateMachine<PlayerController>(this);
 *         
 *         // Optional: Set a global state that runs every frame regardless of current state
 *         stateMachine.SetGlobalState(new PlayerGlobalState());
 *         
 *         // Start in idle state
 *         stateMachine.ChangeState(new PlayerIdleState());
 *     }
 *     
 *     void Update()
 *     {
 *         stateMachine.Update(); // Calls Execute() on global and current states
 *     }
 * }
 * 
 * public class PlayerIdleState : State<PlayerController>
 * {
 *     public override void Enter()
 *     {
 *         Debug.Log("Player is idle");
 *     }
 *     
 *     public override void Execute()
 *     {
 *         // Check for movement input
 *         if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
 *         {
 *             owner.stateMachine.ChangeState(new PlayerWalkState());
 *         }
 *         
 *         // Check for jump
 *         if (Input.GetKeyDown(KeyCode.Space))
 *         {
 *             owner.stateMachine.ChangeState(new PlayerJumpState());
 *         }
 *     }
 * }
 * 
 * public class PlayerWalkState : State<PlayerController>
 * {
 *     public override void Enter()
 *     {
 *         Debug.Log("Player is walking");
 *     }
 *     
 *     public override void Execute()
 *     {
 *         // Handle movement
 *         float h = Input.GetAxis("Horizontal");
 *         float v = Input.GetAxis("Vertical");
 *         Vector3 movement = new Vector3(h, 0, v) * 5f * Time.deltaTime;
 *         owner.transform.Translate(movement);
 *         
 *         // Return to idle if no input
 *         if (h == 0 && v == 0)
 *         {
 *             owner.stateMachine.ChangeState(new PlayerIdleState());
 *         }
 *         
 *         // Jump while walking
 *         if (Input.GetKeyDown(KeyCode.Space))
 *         {
 *             owner.stateMachine.ChangeState(new PlayerJumpState());
 *         }
 *     }
 * }
 * 
 * public class PlayerJumpState : State<PlayerController>
 * {
 *     public override void Enter()
 *     {
 *         owner.rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
 *     }
 *     
 *     public override void Execute()
 *     {
 *         // Check if landed
 *         if (owner.transform.position.y <= 0.5f)
 *         {
 *             // Return to previous state (Idle or Walk)
 *             owner.stateMachine.RevertToPreviousState();
 *         }
 *     }
 * }
 * 
 * public class PlayerGlobalState : State<PlayerController>
 * {
 *     // This Execute() runs every frame, regardless of current state
 *     public override void Execute()
 *     {
 *         // Example: Check for pause menu in any state
 *         if (Input.GetKeyDown(KeyCode.Escape))
 *         {
 *             Debug.Log("Opening pause menu...");
 *         }
 *     }
 * }
 * 
 * TIPS:
 * -----
 * - Keep states focused on a single behavior or responsibility
 * - Use Enter() for initialization (play animations, set flags)
 * - Use Execute() for frame-by-frame logic and state transition checks
 * - Use Exit() for cleanup (stop sounds, reset variables)
 * - Use globalState for logic that should run in ALL states (UI, universal input)
 * - Set allowChangeToSameState = false to prevent redundant Enter/Exit calls
 */


public class StateMachine<T>
{
	public bool allowChangeToSameState = true;
	public T owner { get; private set; }
	public State<T> currentState { get; private set; }
	public State<T> prevState { get; private set; }
	public State<T> globalState { get; private set; }

	public StateMachine(T aOwner){owner = aOwner; }

	private void SetCurrentState(State<T> state)
	{
		currentState = state; 
		currentState.owner = owner; 
	}
	
	private void SetPrevState(State<T> state) => prevState = state;
	public void SetGlobalState(State<T> state) => globalState = state;
	
	public void Update()
	{
		if (globalState is not null) 
			globalState.Execute();
		
		if (currentState is not null) 
			currentState.Execute();
	}

	public void ChangeState(State<T> newState)
	{
		if (!allowChangeToSameState && currentState == newState) 
			return;
		
		if (currentState != null)
		{
			SetPrevState(currentState);
			currentState.Exit();
		}
		
		SetCurrentState(newState);
		currentState.Enter();
	}

	public void RevertToPreviousState() => ChangeState(prevState);

}
