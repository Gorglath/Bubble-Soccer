using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleSoccer
{
    public enum FSMState
    {
        None,
        Idle,
        ChasePlayer,
        ChaseBall,
        Pushed,
        TryToScore,
        Kick,
        Died
    }

    public class BaseEnemy : FSM
    {
        const float pathUpdateMoveThreshold = 0.07f;
        const float minPathUpdateTime = 0.1f;

        public FSMState CurrentState;

        public Transform Target;
        public GameObject ShadowReference;

        public float Speed = 20f;
        public float TurnSpeed = 3f;
        public float TurnDistance = 5f;
        public float StoppingDistance = 10f;
        public bool Dead = false;
        public bool springed = false;
        public bool IsEnemyBig = false;
        [SerializeField]
        private float ballKickForce = 10f;
        
        [SerializeField]
        private float distanceToStartChasing = 10f;

        [SerializeField]
        private float distanceToKickBall = 3f;

        [SerializeField]
        private LayerMask groundLayer;
        [SerializeField]
        private Transform ballPosition;

        [SerializeField]
        private Animator bubbleAnimator;

        [SerializeField]
        private GameObject impactPS;

        [SerializeField]
        private PhysicMaterial wallMaterial, defaultMaterial;

        [SerializeField]
        private Collider enemyCollider;
        [SerializeField]
        private float enemyPushForce = 10f;

        [SerializeField]
        private float pushForceMultiplier = 1f;
        //helpers
        private CustomPath path;
        private CameraController cameraController;
        private Rigidbody rigidbody;
        private Animator animator;
        private Rigidbody ballRigidbodyReference;
        private GameObject ballInSceneReference;
        private GameObject playerReference;
        private GameObject playerGoalReference;
        private GameObject heldBallReference;
        private Ray ray;
        private RaycastHit hit;
        private Vector3 ballKickDirection;
        private Vector3 pushingDirection;
        private bool haveBall = false;
        private bool haveDetectedBall = false;
        private bool haveDetectedPlayer = false;
        private bool haveDetectedGoal = false;
        private bool appliedKnockback = false;
        private bool canMove = true;
        private bool appliedPush = false;
        private bool alreadyFalling = false;
        private bool isAlreadyDetectedBallOnce = false;
        private bool isAlreadyDetectedPlayerOnce = false;
        private float forceToApply = 10f;
        private float timeToCheckMovement = 0.3f;
        private float timeBeforeStartFalling = 0.1f;
        private float fallingResetCounter = 0f;
        private float fallingCounter = 0f;
        private float pushStateCounter = 0f;
        private float counter = 0f;
        private float distanceToBall = 0f;
        private float distanceToPlayer = 0f;
        private float timeOutForPushState = 1f;

        public void InitializeEnemy()
        {
            Invoke("DelayBallFind", 1f);
            animator = GetComponent<Animator>();
            playerReference = GameObject.Find("Player(Clone)");
            playerGoalReference = GameObject.FindGameObjectWithTag("PlayerGoal");
            cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            CurrentState = FSMState.None;
            rigidbody = GetComponent<Rigidbody>();
        }
        
        private void DelayBallFind()
        {
            ballInSceneReference = GameObject.FindGameObjectWithTag("Ball");
            CurrentState = FSMState.Idle;
        }
        protected override void FSMUpdate()
        {
            if (GameManager.Instance.LevelStarted)
            {
                CheckFloor();

                if (!canMove)
                {
                    return;
                }

                switch (CurrentState)
                {
                    case FSMState.Idle:
                        UpdateIdleState();
                        break;
                    case FSMState.ChasePlayer:
                        UpdateChasePlayerState();
                        break;
                    case FSMState.ChaseBall:
                        UpdateChaseBallState();
                        break;
                    case FSMState.Pushed:
                        UpdatePushedState();
                        break;
                    case FSMState.TryToScore:
                        UpdateTryToScoreState();
                        break;
                    case FSMState.Kick:
                        UpdateKickBallState();
                        break;
                    case FSMState.Died:
                        UpdateDiedState();
                        break;
                }
            }
        }

        protected void UpdateChasePlayerState()
        {
            //if the ball is range and not held by the player.
            if (!haveDetectedPlayer)
            {
                haveDetectedPlayer = true;
                Target = playerReference.transform;
                canMove = true;
                StopCoroutine("UpdatePath");
                StartCoroutine("UpdatePath");
            }

            distanceToPlayer = Vector3.Distance(playerReference.transform.position, transform.position);
            distanceToBall = Vector3.Distance(ballInSceneReference.transform.position, transform.position);

            if (!IsEnemyBig)
            {
                if (!ballInSceneReference.GetComponent<Ball>().isCurrentlyHeld && (distanceToBall < distanceToPlayer && distanceToBall < distanceToStartChasing))
                {
                    haveDetectedPlayer = false;
                    CurrentState = FSMState.ChaseBall;
                }
            }
            else
            {
                if (!ballInSceneReference.GetComponent<Ball>().isCurrentlyHeld && (distanceToBall < distanceToStartChasing && distanceToPlayer > distanceToStartChasing))
                {
                    haveDetectedPlayer = false;
                    CurrentState = FSMState.ChaseBall;
                }
            }


            if (haveBall)
            {
                CurrentState = FSMState.TryToScore;
            }

            
            if(rigidbody.velocity.sqrMagnitude < 0.01f)
            {
                counter += Time.deltaTime;
                if(counter >= timeToCheckMovement)
                {
                    counter = 0f;
                    haveDetectedPlayer = false;
                    StopCoroutine("FollowPath");
                }
            }
        }

        protected void UpdateChaseBallState()
        {
            //if the ball is range and not held by the player.
            if (!haveDetectedBall)
            {
                haveDetectedBall = true;
                Target = ballInSceneReference.transform;
                canMove = true;
                StopCoroutine("UpdatePath");
                StartCoroutine("UpdatePath");
            }

            if (ballInSceneReference.GetComponent<Ball>().isCurrentlyHeld)
            {
                if(ballInSceneReference.GetComponent<Ball>().BallHolder.tag == "Enemy")
                {
                    canMove = false;
                    haveDetectedBall = false;
                    animator.SetBool("Running", false);
                    CurrentState = FSMState.Idle;
                }
                else
                {
                    haveDetectedBall = false;
                    CurrentState = FSMState.ChasePlayer;
                }
            }

            if (haveBall)
            {
                CurrentState = FSMState.TryToScore;
            }
            
            if(rigidbody.velocity.sqrMagnitude < 0.01f)
            {
                counter += Time.deltaTime;
                if(counter >= timeToCheckMovement)
                {
                    counter = 0f;
                    haveDetectedBall = false;
                    StopCoroutine("FollowPath");
                }
            }
        }

        protected void UpdateKickBallState()
        {
            //if in position to score a goal.
            canMove = false;
            KickBall();
        }

        protected void UpdateTryToScoreState()
        {
            //if you got the ball.

            if (!haveDetectedGoal)
            {
                haveDetectedGoal = true;
                Target = playerGoalReference.transform;
                canMove = true;
                StopCoroutine("UpdatePath");
                StartCoroutine("UpdatePath");
            }

            float distanceToGoal = Vector3.Distance(playerGoalReference.transform.position, transform.position);
            if ((distanceToGoal < distanceToKickBall))
            {
                CurrentState = FSMState.Kick;
            }
        }

        protected void UpdatePushedState()
        {
            //If You Were Pushed.
            if (!appliedKnockback)
            {
                appliedKnockback = true;
                GotPushed((transform.position - playerReference.transform.position).normalized,forceToApply);
            }

            if (canMove)
            {
                CurrentState = FSMState.Idle;
            }
        }

        protected void UpdateDiedState()
        {
            //If You Died.{?}
        }

        protected void UpdateIdleState()
        {
            distanceToPlayer = Vector3.Distance(playerReference.transform.position, transform.position) ;
            if ((distanceToPlayer < distanceToStartChasing || isAlreadyDetectedPlayerOnce))
            {
                isAlreadyDetectedPlayerOnce = true;

                haveDetectedPlayer = false;
                animator.SetBool("Running", true);
                CurrentState = FSMState.ChasePlayer;
            }
            
            distanceToBall = Vector3.Distance(ballInSceneReference.transform.position, transform.position);
            if ((distanceToBall < distanceToStartChasing || isAlreadyDetectedBallOnce) && !ballInSceneReference.GetComponent<Ball>().isCurrentlyHeld && !IsEnemyBig)
            {
                haveDetectedBall = false;
                isAlreadyDetectedBallOnce = true;
                animator.SetBool("Running", true);
                CurrentState = FSMState.ChaseBall;
            }

        }

        private void CheckFloor()
        {
            ray = new Ray(transform.position + Vector3.up, Vector3.down);
            if (!Physics.Raycast(ray, out hit, 100f, groundLayer))
            {
                fallingResetCounter = 0f;

                if (!alreadyFalling)
                {
                    fallingCounter += Time.deltaTime;
                    if (fallingCounter >= timeBeforeStartFalling)
                    {
                        fallingCounter = 0f;
                        alreadyFalling = true;
                        animator.SetBool("Falling", true);
                        enemyCollider.material = wallMaterial;
                        ShadowReference.SetActive(false);
                        StartCoroutine("RemoveIndicator");
                    }
                }
            }
            else
            {
                fallingCounter = 0f;
                if (alreadyFalling)
                {
                    fallingResetCounter += Time.deltaTime;
                    if (fallingResetCounter >= timeBeforeStartFalling)
                    {
                        fallingResetCounter = 0f;
                        alreadyFalling = false;
                        animator.SetBool("Falling", false);
                        enemyCollider.material = defaultMaterial;
                        ShadowReference.SetActive(true);
                    }
                }
            }
        }

        private void RotateBall()
        {
            heldBallReference.transform.position = ballPosition.position;

            ballRigidbodyReference.AddTorque(transform.right);
        }

        public void PickUpBall(GameObject ball)
        {
            haveBall = true;
            
            haveDetectedBall = false;

            haveDetectedGoal = false;

            haveDetectedPlayer = false;

            heldBallReference = ball;

            ballRigidbodyReference = heldBallReference.GetComponent<Rigidbody>();

            heldBallReference.transform.parent = ballPosition;

            heldBallReference.transform.localPosition = Vector3.zero;

            CurrentState = FSMState.TryToScore;
        }

        private void KickBall()
        {
            animator.SetBool("Kicking", true);

            haveBall = false;

            heldBallReference.transform.parent = null;

            StartCoroutine("DelayKick");
        }
        public void DropBall()
        {
            haveBall = false;

            haveDetectedBall = false;

            haveDetectedGoal = false;

            haveDetectedPlayer = false;

            ballRigidbodyReference = null;

            heldBallReference.transform.parent = null;
            
            heldBallReference.GetComponent<Ball>().Invoke("ResetKicked", 1f);

            heldBallReference.GetComponent<Ball>().BallHolder = null;

        }
        public void GotPushed(Vector3 direction,float playerPushForce)
        {
            canMove = false;
            appliedKnockback = false;
            StopCoroutine("UpdatePath");
            StopCoroutine("FollowPath");
            bubbleAnimator.SetBool("Pushed", true);
            Invoke("ResetBubbleAnimation", 0.3f);
            StartCoroutine(WasPushed(direction, playerPushForce));
        }

        IEnumerator WasPushed(Vector3 direction,float playerPushForce)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(-direction), 360f);
            animator.SetBool("GotPushed", true);
            if (!springed)
            {
                direction.y = 0;
            }

            if (haveBall)
            {
                DropBall();
            }

            animator.SetBool("Running", false);
            CurrentState = FSMState.Idle;
            float timeToCompletePush = 1f;
            ray = new Ray(transform.position + (direction.normalized * 2f) + Vector3.up, Vector3.down);
            if (!Physics.Raycast(ray, out hit, 100, groundLayer))
            {
                rigidbody.AddForce(direction.normalized * 1.5f * playerPushForce, ForceMode.Impulse); 

                Invoke("ResetGotPushed", 1f);
            }
            else
            {
                rigidbody.AddForce(direction.normalized * playerPushForce, ForceMode.Impulse);
                while (timeToCompletePush > 0)
                {
                    timeToCompletePush -= Time.deltaTime;
                    yield return null;
                }


                canMove = true;
                animator.SetBool("GotPushed", false);
                appliedPush = false;

            }
        }
        public void OnPathFound(Vector3[] wayPoints, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                path = new CustomPath(wayPoints, transform.position, TurnDistance, StoppingDistance);
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        }

        IEnumerator UpdatePath()
        {
            if (Time.timeSinceLevelLoad < 0.3f)
            {
                yield return new WaitForSeconds(0.3f);
            }
            PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, OnPathFound));

            float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
            Vector3 targetPosOld = Target.position;

            while (true)
            {
                yield return new WaitForSeconds(minPathUpdateTime);
                if (Target)
                {
                    if ((Target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold && canMove)
                    {
                        PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, OnPathFound));
                        targetPosOld = Target.position;
                    }
                }


            }
        }
        IEnumerator FollowPath()
        {
            bool followingPath = true;
            int pathIndex = 0;
            Vector3 lookPosition = path.LookPoints[0];
            lookPosition.y = transform.position.y;
            transform.LookAt(lookPosition);

            float speedPercent = 1;

            while (followingPath)
            {
                Vector2 position2D = new Vector2(transform.position.x, transform.position.z);
                while (path.TurnBoundaries[pathIndex].HasCrossedLine(position2D))
                {
                    if (pathIndex == path.FinishLineIndex)
                    {
                        followingPath = false;
                        break;
                    }
                    else
                    {
                        pathIndex++;
                    }
                }

                if (followingPath && GameManager.Instance.LevelStarted)
                {
                    //if (pathIndex >= path.slowDownIndex && StoppingDistance > 0)
                    //{
                    //    speedPercent = Mathf.Clamp01(path.TurnBoundaries[path.FinishLineIndex].DistanceFromPoint(position2D) / StoppingDistance);
                    //    if (speedPercent < 0.01)
                    //    {
                    //        followingPath = false;
                    //    }
                    //}
                    Quaternion targetRotation = Quaternion.LookRotation(path.LookPoints[pathIndex] - transform.position);
                    targetRotation.x = 0;
                    targetRotation.z = 0;
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);

                    if (haveBall)
                    {
                        RotateBall();
                    }
                    transform.Translate(Vector3.forward * Time.deltaTime * Speed * speedPercent, Space.Self);
                }
                yield return null;
            }
        }
        IEnumerator DelayKick()
        {
            yield return null;

            cameraController.EnemyKickedLostGame(playerGoalReference.transform.position);

            ballKickDirection = transform.forward.normalized + Vector3.up * 0.1f;

            ballRigidbodyReference.AddForce(ballKickDirection.normalized * ballKickForce, ForceMode.Impulse);

            ballRigidbodyReference = null;

            heldBallReference.GetComponent<Ball>().GotKicked();
            heldBallReference.GetComponent<Ball>().Kicked = true;

            heldBallReference.GetComponent<Ball>().Invoke("ResetKicked", 1f);

            CurrentState = FSMState.Idle;
            //canMove = true;
            Invoke("ResetKicked", 0.2f);
        }

        IEnumerator RemoveIndicator()
        {
            yield return new WaitForSeconds(0.2f);
            if (alreadyFalling)
            {
                GameManager.Instance.UIManager.RemoveEnemyIndicator();
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if((other.tag == "Ball" && other.GetComponent<Ball>().Kicked) || other.tag == "Player"
                && !other.GetComponent<PlayerController>().IsHaveBall)
            {
                if (!appliedPush)
                {
                    //Destroy(Instantiate(impactPS, other.ClosestPoint(transform.position), Quaternion.LookRotation(transform.position - other.transform.position)), 2f);
                    appliedPush = true;
                    CurrentState = FSMState.Pushed;
                    if (other.tag == "Player")
                    {
                        forceToApply = playerReference.GetComponent<PlayerController>().PlayerPushForce;

                        if (!other.GetComponent<PlayerController>().IsShielded)
                        {
                            if (other.GetComponent<PlayerController>().IsPushing)
                            {
                                pushingDirection = (other.transform.position - transform.position).normalized;
                                pushingDirection.y = 0f;
                                other.GetComponent<PlayerController>().GotPushed(pushingDirection * pushForceMultiplier, enemyPushForce);
                            }
                            else
                            {
                                pushingDirection = (other.transform.position - transform.position).normalized;
                                pushingDirection.y = 0f;
                                other.GetComponent<PlayerController>().GotPushed(pushingDirection * (pushForceMultiplier/3f), enemyPushForce);
                            }
                        }
                    }
                    else if(other.tag == "Ball")
                    {
                        forceToApply = playerReference.GetComponent<PlayerController>().PlayerPushForce * 0.8f;
                        ballInSceneReference.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        Vector3 ballRicoilDirection = (transform.position - other.transform.position).normalized;
                        ballInSceneReference.GetComponent<Rigidbody>().AddForce(ballRicoilDirection * (ballKickForce/2), ForceMode.VelocityChange);
                    }
                }
            }
            else if(other.tag == "Player" && other.GetComponent<PlayerController>().IsHaveBall)
            {
                if (!appliedPush)
                {
                    //Destroy(Instantiate(impactPS, other.ClosestPoint(transform.position), Quaternion.LookRotation(transform.position - other.transform.position)), 2f);

                    forceToApply = other.GetComponent<PlayerController>().PlayerPushForce;
                    appliedPush = true;
                    CurrentState = FSMState.Pushed;

                    if (!other.GetComponent<PlayerController>().IsShielded)
                    {
                        other.GetComponent<PlayerController>().GotPushed((other.transform.position - transform.position).normalized * pushForceMultiplier, enemyPushForce);
                    }
                }
            }
        }
        private void ResetGotPushed()
        {
            animator.SetBool("GotPushed", false);
        }
        private void ResetKicked()
        {
            animator.SetBool("Kicking", false);
        }
        private void ResetBubbleAnimation()
        {
            if (bubbleAnimator)
            {
                bubbleAnimator.SetBool("Pushed", false);
            }
        }
        public void ResetFallingVariable()
        {
            animator.SetBool("Loop", true);
        }
        private void OnDrawGizmos()
        {
            if (path != null)
            {
                foreach (Vector3 waypoint in path.LookPoints)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(waypoint, Vector3.one * (0.1f));
                }
            }

            //Gizmos.DrawWireSphere(transform.position, distanceToStartChasing);
            Gizmos.DrawRay(ray);
        }
        private void OnDestroy()
        {
            DestroyImmediate(ShadowReference);
        }
    }
}
