using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace BubbleSoccer
{
    
    public class PlayerController : MonoBehaviour
    {
        [HideInInspector]
        public bool IsHaveBall = false;

        [HideInInspector] 
        public bool IsShielded = false;

        [HideInInspector] 
        public bool IsSpringed = false;

        [HideInInspector] 
        public bool IsPushing = false;
        
        [Header("Misc")]
        public float PlayerPushForce = 10f;
        public GameObject ShadowReference;

        [Header("Variables")]
        [SerializeField]
        private float movementSpeed = 20f;

        [SerializeField]
        private float turningSpeed = 20f;

        [SerializeField]
        private float ballKickForce = 10f;

        [SerializeField]
        private float pushForceMultiplier = 1f;

        [SerializeField]
        private float ballMovementSpeedPenaltyMultiplier = 0.8f;

        [SerializeField]
        private float ballKickForceMultiplier = 1f;

        [SerializeField]
        private float minDistanceToStartMoving = 10f;
        
        [SerializeField]
        private LayerMask groundLayer;


        [SerializeField]
        private float timeBeforeCanPush = 1f;

        [SerializeField]
        private float playerPushWithoutBallForce = 4f;
        
        [Header("Referenses")]
        [SerializeField]
        private Text debugText;

        [SerializeField]
        private Transform ballPosition;

        [SerializeField]
        private GameObject ballReference;

        [SerializeField]
        private Collider playerCollider;

        [SerializeField]
        private PhysicMaterial wallMaterial, defaultMaterial;

        [SerializeField]
        private Animator bubbleAnimator;

        [SerializeField]
        private ParticleSystem boostPS;

        [SerializeField]
        private ParticleAtrractor particleAttractor;

        [SerializeField]
        private Color powerTrailColor, speedTrailColor, shieldTrailColor;

        //helpers
        private Grid grid;
        private Node placeHolder;
        private Rigidbody rigidbody;
        private Rigidbody ballRigidbodyReference;
        private Animator animator;
        private Transform cacheTransform;
        private ParticleSystem particleSystemPlaceHolder;
        private Ray ray;
        private RaycastHit hit;
        private Vector2 firstTouchOffset;
        private Vector3 ballKickDirection;
        private Vector3 gravity;
        private float forwardAngleArea;
        private float backAngleArea;
        private float leftAngleArea;
        private float rightAngleArea;
        private float xMovement;
        private float yMovement;
        private float currentVelocity;
        private float initialMovementSpeed;
        private float initialPushForce;
        private float timeBeforeCanPushReset;
        private float angleY;
        private float angleZ;
        private float angleX;
        private float targetAngle;
        private float timeBeforeStartFalling = 0.1f;
        private float fallingResetCounter = 0f;
        private float counter = 0f;
        private bool canMove = true;
        private bool canPush = false;
        private bool firstTouch = true;
        private bool alreadyFalling = false;
        private bool standing = true;
        private void OnEnable()
        {
            animator = GetComponent<Animator>();
            GameManager.Instance.SetPlayerAnimator(animator);
            cacheTransform = transform;
            rigidbody = GetComponent<Rigidbody>();
            gravity.y = -70f;
            initialMovementSpeed = movementSpeed;
            initialPushForce = PlayerPushForce;
            if (Input.touchCount != 0)
            {
                firstTouchOffset = Input.GetTouch(0).position;
            }
            Camera.main.GetComponent<CameraController>().player = transform;
            Camera.main.GetComponent<CameraController>().ResetCamera();

            //debugText = GameObject.Find("Text").GetComponent<Text>();
            grid = GameObject.Find("A-Star").GetComponent<Grid>();
        }
        private void OnDisable()
        {
            Camera.main.GetComponent<CameraController>().player = null;
        }
        private void Update()
        {
            CheckFloor();

            if ((Input.touchCount != 1 && !GameManager.Instance.DebugMode) || ((!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0)) && GameManager.Instance.DebugMode))
            {
                if (!IsPushing)
                {
                    animator.SetBool("Running", false);
                }
                if (IsHaveBall)
                {
                    KeepBallInPlace();
                }
                return;
            }

            if (!alreadyFalling)
            {
                if (GameManager.Instance.DebugMode)
                {
                    CheckInputDebug();
                }
                else
                {
                    CheckInput();
                }
            }
        }

        private void CheckFloor()
        {
            ray = new Ray(transform.position + Vector3.up, Vector3.down);
            if(!Physics.Raycast(ray,out hit, 100f, groundLayer))
            {
                fallingResetCounter = 0f;

                if (!alreadyFalling)
                {
                    counter += Time.deltaTime;
                    if (counter >= timeBeforeStartFalling)
                    {
                        counter = 0f;
                        alreadyFalling = true;
                        animator.SetBool("Falling", true);
                        playerCollider.material = wallMaterial;
                    }
                }
            }
            else
            {
                counter = 0f;
                if (alreadyFalling)
                {
                    fallingResetCounter += Time.deltaTime;
                    if (fallingResetCounter >= timeBeforeStartFalling)
                    {
                        fallingResetCounter = 0f;
                        alreadyFalling = false;
                        animator.SetBool("Falling", false);
                        playerCollider.material = defaultMaterial;
                    }
                }
            }
        }
        public void ResetFallingVariable()
        {
            animator.SetBool("Loop", true);
        }
        private void CheckInput()
        {
            if (!canMove)
            {
                if (IsHaveBall)
                {
                    KeepBallInPlace();
                }
                return;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (firstTouch)
                {
                    firstTouch = false;
                    GameManager.Instance.StartLevel();
                }
                firstTouchOffset = Input.GetTouch(0).position;

            }
            else if((Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) && GameManager.Instance.LevelStarted)
            {
                MoveCharacter();
                if (IsHaveBall)
                {
                    RotateBall();
                }
            }
            else if(Input.GetTouch(0).phase == TouchPhase.Ended && GameManager.Instance.LevelStarted && !standing)
            {
                if (IsHaveBall)
                {
                    KickBall();
                }
                //else if(canPush)
                //{
                //    canPush = false;
                //    StartCoroutine(Pushing(transform.forward, playerPushWithoutBallForce));
                //}
            }
        }
        private void CheckInputDebug()
        {
            if (!canMove)
            {
                if (IsHaveBall)
                {
                    KeepBallInPlace();
                }
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (firstTouch)
                {
                    firstTouch = false;
                    GameManager.Instance.StartLevel();
                }
                firstTouchOffset = Input.mousePosition;

            }
            else if (Input.GetMouseButton(0) && GameManager.Instance.LevelStarted)
            {
                MoveCharacter();
                if (IsHaveBall)
                {
                    RotateBall();
                }
            }
            else if (Input.GetMouseButtonUp(0) && GameManager.Instance.LevelStarted)
            {
                if (IsHaveBall)
                {
                    KickBall();
                }
                else if (canPush)
                {
                    canPush = false;
                    StartCoroutine(Pushing(transform.forward, playerPushWithoutBallForce));
                }
            }
        }
        private float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        private void KeepBallInPlace()
        {
            ballReference.transform.position = ballPosition.position;
        }
        private void MoveCharacter()
        {
            if (!IsHaveBall)
            {
                timeBeforeCanPush -= Time.deltaTime;
                if (timeBeforeCanPush <= 0)
                {
                    timeBeforeCanPush = 1f;
                    canPush = true;
                }
            }

            Vector2 direction;

            if (GameManager.Instance.DebugMode)
            {
                direction = Input.mousePosition;
                direction -= firstTouchOffset;
            }
            else
            {
                direction = Input.GetTouch(0).position - firstTouchOffset;
            }

            if ((direction.x < minDistanceToStartMoving && direction.x > -minDistanceToStartMoving) && (direction.y < minDistanceToStartMoving && direction.y > -minDistanceToStartMoving))
            {
                animator.SetBool("Running", false);
                rigidbody.velocity = new Vector3(0,rigidbody.velocity.y,0);
                standing = true;
                return;
            }
            else
            {
                animator.SetBool("Running", true);
                standing = false;
            }

            float distanceFromCenter;

            if (GameManager.Instance.DebugMode)
            {
                distanceFromCenter = Mathf.Clamp(Vector2.Distance(firstTouchOffset, Input.mousePosition), minDistanceToStartMoving, 20f);
            }
            else
            {
                distanceFromCenter = Mathf.Clamp(Vector2.Distance(firstTouchOffset, Input.GetTouch(0).position), minDistanceToStartMoving, 20f);
            }
            
            Vector2 forceToApply = new Vector2(Mathf.Clamp(direction.x, -200f, 200f), Mathf.Clamp(direction.y, -200f, 200f));

            distanceFromCenter = Remap(distanceFromCenter, minDistanceToStartMoving, 20f, 0f, 1.5f);
            xMovement = Remap(forceToApply.x, -200f, 200f, -1f, 1f);
            yMovement = Remap(forceToApply.y, -200f, 200f, -1f, 1f);
            
            Vector3 newPos = new Vector3(xMovement, -1, yMovement);

            targetAngle = Mathf.Atan2(newPos.x, newPos.z) * Mathf.Rad2Deg;

            angleY = Mathf.SmoothDampAngle(cacheTransform.eulerAngles.y, targetAngle, ref currentVelocity, 0.05f);
            
            cacheTransform.rotation = Quaternion.Euler(0f, angleY, 0f);


            Vector3 newDirection = new Vector3(cacheTransform.forward.x, 0 , cacheTransform.forward.z);
            
            rigidbody.velocity = new Vector3(newDirection.normalized.x * movementSpeed * distanceFromCenter, rigidbody.velocity.y,newDirection.normalized.z * movementSpeed * distanceFromCenter);
            
        }

        private void RotateBall()
        {
            if (!standing)
            {
                ballReference.transform.position = ballPosition.position;

                ballRigidbodyReference.AddTorque(new Vector3(yMovement * 5, 0, -xMovement * 5));
            }
            else
            {
                ballReference.transform.position = ballPosition.position;
                ballRigidbodyReference.angularVelocity = Vector3.zero;
            }
        }

        

        public void PickUpBall(GameObject ball)
        {
            IsHaveBall = true;

            IsShielded = true;

            Invoke("ResetShield", 0.5f);

            ballReference = ball;

            ballRigidbodyReference = ballReference.GetComponent<Rigidbody>();

           // ballRigidbodyReference.isKinematic = true;


            ballRigidbodyReference.transform.parent = ballPosition;

            ballRigidbodyReference.transform.localPosition = Vector3.zero;

        }
        
        public void ApplyBoost(Boost boost,ParticleSystem boostParticleSystem)
        {
            particleSystemPlaceHolder = boostParticleSystem;
            particleSystemPlaceHolder.Play();
            Invoke("StartAttractingParticles", 0.5f);
            boostPS.Play();
            ParticleSystem.MainModule main = boostPS.main;
            switch (boost.Type)
            {
                case BoostType.MovementBoost:
                    movementSpeed *= boost.BoostMultiplier;
                    main.startColor = speedTrailColor;
                    Invoke("ResetMovementBoost", 10f);
                    break;
                case BoostType.ShieldBoost:
                    IsShielded = true;
                    main.startColor = shieldTrailColor;
                    Invoke("ResetShield", 10f);
                    break;
                case BoostType.PushingBoost:
                    PlayerPushForce *= boost.BoostMultiplier;
                    main.startColor = powerTrailColor;
                    Invoke("ResetPushBoost", 10f);
                    break;
            }
            Destroy(boost.gameObject,5f);
        }

        private void StartAttractingParticles()
        {
            particleAttractor.ParticleSystemToAttract = particleSystemPlaceHolder;
            particleAttractor.enabled = true;
        }

        public void KickBall()
        {
            animator.SetBool("Kicking", true);

            animator.SetBool("Running", false);

            IsHaveBall = false;

            ballReference.transform.parent = null;
            
            StartCoroutine("DelayKick");
        }
        public void DropBall()
        {
            IsHaveBall = false;

            ballReference.transform.parent = null;

            ballRigidbodyReference = null;

            //ballReference.GetComponent<Rigidbody>().isKinematic = false;

            ballReference.GetComponent<Ball>().Invoke("ResetKicked", 1f);

            ballReference.GetComponent<Ball>().BallHolder = null;
        }
        public void GotPushed(Vector3 direction,float enemyPushForce)
        {
            bubbleAnimator.SetBool("Pushed", true);
            Invoke("ResetBubbleAnimation", 0.3f);
            StartCoroutine(WasPushed(direction,enemyPushForce));
        }

        IEnumerator WasPushed(Vector3 direction,float enemyPushForce)
        {
            animator.SetBool("Running", false);
            animator.SetBool("Pushing", false);
            animator.SetBool("GotPushed", true);
            canMove = false;
            direction.y = cacheTransform.position.normalized.y;
            cacheTransform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(-direction), 360f);
            if (!IsSpringed)
            {
                direction.y = 0;
            }
            if (IsHaveBall)
            {
                DropBall();
            }

            direction.y = 0f;
            float timeToCompletePush = 1f;
            ray = new Ray(cacheTransform.position + direction.normalized + Vector3.up, Vector3.down);
            if(!Physics.Raycast(ray,out hit, 100, groundLayer))
            {
                rigidbody.AddForce(direction.normalized * 0.8f * enemyPushForce, ForceMode.Impulse);

                Invoke("ResetGotPushed", 1f);
            }
            else
            {
                rigidbody.AddForce(direction.normalized * enemyPushForce, ForceMode.Impulse);

                while (timeToCompletePush > 0)
                {
                    timeToCompletePush -= Time.deltaTime;
                    yield return null;
                }

               // placeHolder = grid.NodeFromWorldPoint(cacheTransform.position);
                //foreach (Node neighbor in grid.GetNeighbours(placeHolder))
                //{
                //    foreach (Node neighbor2 in grid.GetNeighbours(neighbor))
                //    {
                //        foreach (Node neighbor3 in grid.GetNeighbours(neighbor2))
                //        {
                //            neighbor3.Walkable = false;
                //        }
                //        neighbor2.Walkable = false;
                //    }
                //    neighbor.Walkable = false;
                //}

                //placeHolder.Walkable = false;
                canMove = true;
                animator.SetBool("GotPushed", false);

                //Invoke("ResetNode", 0.1f);
            }
        }
        IEnumerator Pushing(Vector3 direction,float forceAmount)
        {
            IsPushing = true;
            animator.SetBool("Running", false);
            animator.SetBool("Pushing", true);
            canMove = false;
            timeBeforeCanPush = timeBeforeCanPushReset;
            direction.y = 0;
            if (IsHaveBall)
            {
                DropBall();
            }


            float timeToCompletePush = 1f;
            bool isChangeAnimationOnce = false;
            rigidbody.AddForce(direction.normalized * forceAmount, ForceMode.Impulse);
            while (timeToCompletePush > 0)
            {
                timeToCompletePush -= Time.deltaTime;
                if(timeToCompletePush <= 0.2f && !isChangeAnimationOnce)
                {
                    isChangeAnimationOnce = true;
                    animator.SetBool("Pushing", false);
                }
                yield return null;
            }

            canMove = true;
            IsPushing = false;
        }
        
        IEnumerator DelayKick()
        {
            yield return null;

            ballKickDirection = (cacheTransform.transform.forward + Vector3.up * 0.4f).normalized;

            //ballReference.GetComponent<Rigidbody>().isKinematic = false;

            ballRigidbodyReference.AddForce(ballKickDirection * ballKickForce, ForceMode.Impulse);

            ballRigidbodyReference = null;

            ballReference.GetComponent<Ball>().GotKicked();

            ballReference.GetComponent<Ball>().Kicked = true;

            ballReference.GetComponent<Ball>().Invoke("ResetKicked", 1f);

            Invoke("ResetKicked", 0.2f);
        }
        //private void ResetNode()
        //{
        //    foreach (Node neighbor in grid.GetNeighbours(placeHolder))
        //    {
        //        foreach (Node neighbor2 in grid.GetNeighbours(neighbor))
        //        {
        //            foreach (Node neighbor3 in grid.GetNeighbours(neighbor2))
        //            {
        //                neighbor3.Walkable = true;
        //            }
        //            neighbor2.Walkable = true;
        //        }
        //        neighbor.Walkable = true;
        //    }
        //    placeHolder.Walkable = true;
        //} 
        private void ResetGotPushed()
        {
            animator.SetBool("GotPushed", false);
            canMove = true;
        }
    private void ResetShield()
        {
            IsShielded = false;

            boostPS.Stop();
        }
        private void ResetMovementBoost()
        {
            movementSpeed = initialMovementSpeed;

            boostPS.Stop();
        }
        private void ResetPushBoost()
        {
            PlayerPushForce = initialPushForce;


            boostPS.Stop();
        }
        private void ResetKicked()
        {
            animator.SetBool("Kicking", false);
        }
        private void ResetBubbleAnimation()
        {
            bubbleAnimator.SetBool("Pushed", false);
        }
        private void OnDestroy()
        {
            DestroyImmediate(ShadowReference);
        }
    }
}
