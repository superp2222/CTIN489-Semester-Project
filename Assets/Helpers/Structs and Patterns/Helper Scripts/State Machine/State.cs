
/*
 * BASE STATE CLASS FOR STATE MACHINE PATTERN
 * ===========================================
 * 
 * This is the base class for creating states in a finite state machine.
 * Each state has three key methods that control behavior:
 * - Enter(): Called once when transitioning INTO this state
 * - Execute(): Called every frame while IN this state (via StateMachine.Update())
 * - Exit(): Called once when transitioning OUT OF this state
 * 
 * USAGE EXAMPLE:
 * --------------
 * Create concrete state classes that inherit from State<T>, where T is the type
 * of object that owns the state machine (typically a MonoBehaviour but doesn't have to be!).
 * 
 * Example: Simple Enemy AI with Idle and Chase states
 * 
 * public class EnemyController : MonoBehaviour
 * {
 *     StateMachine<EnemyController> stateMachine;
 *     
 *     void Start()
 *     {
 *         stateMachine = new StateMachine<EnemyController>(this);
 *         stateMachine.ChangeState(new IdleState());
 *     }
 *     
 *     void Update()
 *     {
 *         stateMachine.Update(); // Executes current state's Execute() method
 *     }
 * }
 * 
 * public class IdleState : State<EnemyController>
 * {
 *     public override void Enter()
 *     {
 *         Debug.Log("Enemy is now idle");
 *     }
 *     
 *     public override void Execute()
 *     {
 *         // Check if player is nearby
 *         if (Vector3.Distance(owner.transform.position, PlayerPosition) < 5f)
 *         {
 *             owner.stateMachine.ChangeState(new ChaseState());
 *         }
 *     }
 *     
 *     public override void Exit()
 *     {
 *         Debug.Log("Enemy leaving idle state");
 *     }
 * }
 * 
 * public class ChaseState : State<EnemyController>
 * {
 *     public override void Enter()
 *     {
 *         Debug.Log("Enemy is chasing player");
 *     }
 *     
 *     public override void Execute()
 *     {
 *         // Move toward player
 *         owner.transform.position = Vector3.MoveTowards(
 *             owner.transform.position, 
 *             PlayerPosition, 
 *             5f * Time.deltaTime
 *         );
 *         
 *         // If player gets far away, go back to idle
 *         if (Vector3.Distance(owner.transform.position, PlayerPosition) > 10f)
 *         {
 *             owner.stateMachine.ChangeState(new IdleState());
 *         }
 *     }
 * }
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T>
{
    [HideInInspector]
    public T owner;
    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }
}
