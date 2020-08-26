#define DEBUG_RAYS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCollider : MonoBehaviour
{
    #region Internal Types

    struct RaycastOrigins
    {
        public Vector2 bottomLeft;
        public Vector2 bottomRight;

        public Vector2 squeezedTop;
        public Vector2 squeezedLeft;
        public Vector2 squeezedRight;

        public Vector2 innerBottom;
        public Vector2 innerLeft;
        public Vector2 innerRight;
    }

    public class CollisionState
    {
        /// <summary>
        /// -1 Wall to the Left -- 0 No Wall -- 1 Wall to the Right
        /// </summary>
        public int onWall = 0;

        public bool wallAbove = false;
        public bool nearGround = false;
        public bool onGround = false;

        public bool becameOnGroundLastFrame = false;
        public bool onGroundLastFrame = false;

        public bool rightSqueezed = false;
        public bool leftSqueezed = false;
        public bool bottomSqueezed = false;

        public void Reset()
        {
            onWall = 0;
            nearGround = onGround = wallAbove = becameOnGroundLastFrame = false;
            rightSqueezed = leftSqueezed = bottomSqueezed = false;
        }

        public bool Colliding()
        {
            bool collision = onGround;
            collision = onWall != 0 ? true : collision;
            return collision;
        }
    }

    #endregion

    private enum MovementState { Grounded, Upward, Downward }
    private MovementState movementState = MovementState.Grounded;

    private new Transform transform;
    private Rider rider;
    public BoxCollider2D boxCollider;
    public BoxCollider2D inWallCollider;
    public SpriteRenderer spriteRenderer;

    private CollisionState collisionState = new CollisionState();

    public bool onGround { get { return collisionState.onGround; } }
    public bool nearGround { get { return collisionState.nearGround; } }
    public int onWall { get { return collisionState.onWall; } }

    public LayerMask platformMask = 0;

    private RaycastHit2D raycastHit;
    private List<RaycastHit2D> raycastHitsThisFrame = new List<RaycastHit2D>();
    private RaycastOrigins raycastOrigins;

    public Vector2 velocity = Vector2.zero;
    public Vector2 Velocity { get { return velocity; } }

    private float skinWidth = 0.02f;
    public float SkinWidth
    {
        get { return skinWidth; }
        set
        {
            skinWidth = value;
            CalculateDistanceBetweenRays();
        }
    }

    [Range(2, 20)]
    public int totalHorizontalRays = 16;
    [Range(2, 20)]
    public int totalVerticalRays = 8;

    [Tooltip("For Top and Bottom Rays")]
    private float horizontalDistanceBetweenRays = 0f;
    [Tooltip("For Left and Right Rays")]
    private float singleSqueezedVerticalDistanceBetweenRays = 0f;

    [Tooltip("For Top and Bottom Rays")]
    private float doubleSqueezedHorizontalDistanceBetweenRays = 0f;
    [Tooltip("For Left and Right Rays")]
    private float doubleSqueezedVerticalDistanceBetweenRays = 0f;

    private const float WallRaycastDistance = 0.01f;
    private const float NearGroundRaycastDistance = 0.5f;

    [SerializeField]
    private float unitSize = 0.125f;
    [SerializeField]
    private int unitsSqueezed = 2;
    private float squeezeDistance = 0;

    private Bounds boxBounds;

    /// <summary>
    /// Called by objects controller in their start function
    /// </summary>
    public void Setup()
    {
        transform = GetComponent<Transform>();
        rider = GetComponent<Rider>();

        squeezeDistance = unitSize * unitsSqueezed;

        // trigger setter
        SkinWidth = skinWidth;

        // inWallCollider Setup
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(squeezeDistance * -2f);
        inWallCollider.offset = boxCollider.offset;
        inWallCollider.size = bounds.size;
    }

    public delegate void OnSqueeze();
    public OnSqueeze onSqueeze;
    private void Squeeze()
    {
        onSqueeze();
    }

    private void CalculateDistanceBetweenRays()
    {
        float colliderWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - skinWidth;
        float colliderHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - skinWidth;

        horizontalDistanceBetweenRays = colliderWidth / (totalVerticalRays - 1);
        singleSqueezedVerticalDistanceBetweenRays = (colliderHeight - squeezeDistance) / (totalHorizontalRays - 1);

        doubleSqueezedHorizontalDistanceBetweenRays = (colliderWidth - squeezeDistance * 2) / (totalVerticalRays - 1);
        doubleSqueezedVerticalDistanceBetweenRays = (colliderHeight - squeezeDistance * 2) / (totalHorizontalRays - 1);
    }

    private void FindInnerRaycastOrigins()
    {
        boxBounds = boxCollider.bounds;
        //Inset the boxBounds by the skinWidth
        boxBounds.Expand(skinWidth * -1f);

        // Inner Squeezed Variables
        raycastOrigins.innerBottom = new Vector2(boxBounds.min.x, boxBounds.min.y + squeezeDistance);
        raycastOrigins.innerLeft = new Vector2(boxBounds.min.x + squeezeDistance, boxBounds.min.y + squeezeDistance);
        raycastOrigins.innerRight = new Vector2(boxBounds.max.x - squeezeDistance, boxBounds.min.y + squeezeDistance);
    }

    private void FindOuterRaycastOrigins()
    {
        boxBounds = boxCollider.bounds;
        //Inset the boxBounds by the skinWidth
        boxBounds.Expand(skinWidth * -1f);

        // Outer Non-Squeezed Variables
        raycastOrigins.bottomLeft = new Vector2(boxBounds.min.x, boxBounds.min.y);
        raycastOrigins.bottomRight = new Vector2(boxBounds.max.x, boxBounds.min.y);

        // Outer Squeezed Variables
        raycastOrigins.squeezedTop = new Vector2(boxBounds.min.x + squeezeDistance, boxBounds.max.y);
        raycastOrigins.squeezedLeft = new Vector2(boxBounds.min.x, boxBounds.min.y + squeezeDistance);
        raycastOrigins.squeezedRight = new Vector2(boxBounds.max.x, boxBounds.min.y + squeezeDistance);
    }

    private void FindState()
    {
        if (velocity.y > 0)
        {
            movementState = MovementState.Upward;
        }
        else if (velocity.y < 0 && !collisionState.onGroundLastFrame)
        {
            movementState = MovementState.Downward;
        }
        else
        {
            movementState = MovementState.Grounded;
        }
    }

    /// <summary>
    /// attempts to move the object to position + deltaMovement. Any colliders in the way will cause the movement to
    /// stop when run into. Returns velocity.
    /// <param name="velo">Velocity.</param>
    /// </summary>
    public Vector2 Move(Vector2 velo)
    {
        velocity = velo;
        float deltaTime = Time.deltaTime;
        Vector2 deltaMovement = velocity * deltaTime;
        
        collisionState.onGroundLastFrame = collisionState.onGround;

        collisionState.Reset();

        FindState();

        switch (movementState)
        {
            case MovementState.Grounded:
                GroundedMove(ref deltaMovement);
                break;
            case MovementState.Upward:
                UpwardMove(ref deltaMovement);
                break;
            case MovementState.Downward:
                DownwardMove(ref deltaMovement);
                break;
        }

        Vector3 deltaPosition = new Vector3(deltaMovement.x, deltaMovement.y, 0);

        transform.Translate(deltaPosition, Space.World);

        if (!collisionState.onGroundLastFrame && collisionState.onGround)
        {
            collisionState.becameOnGroundLastFrame = true;
        }

        if (deltaTime > 0f)
        {
            velocity = deltaMovement / deltaTime;
        }

        return velocity;
    }

    private void GroundedMove(ref Vector2 deltaMovement)
    {
        float vertDistanceBetweenRays = singleSqueezedVerticalDistanceBetweenRays;

        FindInnerRaycastOrigins();

        HorizontalSqueeze(vertDistanceBetweenRays, raycastOrigins.innerRight, raycastOrigins.innerLeft);

        VerticalSqueeze();


        FindOuterRaycastOrigins();
        Vector2 outerRightOrigin = raycastOrigins.squeezedRight;
        Vector2 outerLeftOrigin = raycastOrigins.squeezedLeft;

        CheckForWall(vertDistanceBetweenRays, outerRightOrigin, outerLeftOrigin);

        if (deltaMovement.x != 0f)
        {
            MoveHorizontally(ref deltaMovement, vertDistanceBetweenRays, outerRightOrigin, outerLeftOrigin);
        }
        
        if(deltaMovement.y != 0f)
        {
            MoveVertically(ref deltaMovement, horizontalDistanceBetweenRays, raycastOrigins.bottomLeft);
        }

        collisionState.nearGround = true;
    }

    private void UpwardMove(ref Vector2 deltaMovement)
    {
        float vertDistanceBetweenRays = singleSqueezedVerticalDistanceBetweenRays;

        FindInnerRaycastOrigins();

        HorizontalSqueeze(vertDistanceBetweenRays, raycastOrigins.innerRight, raycastOrigins.innerLeft);

        VerticalSqueeze();


        FindOuterRaycastOrigins();
        Vector2 outerRightOrigin = raycastOrigins.bottomRight;
        Vector2 outerLeftOrigin = raycastOrigins.bottomLeft;

        CheckForWall(vertDistanceBetweenRays, outerRightOrigin, outerLeftOrigin);

        if (deltaMovement.x != 0f)
        {
            MoveHorizontally(ref deltaMovement, vertDistanceBetweenRays, outerRightOrigin, outerLeftOrigin);
        }

        if(deltaMovement.y != 0f)
        {
            MoveVertically(ref deltaMovement, doubleSqueezedHorizontalDistanceBetweenRays, raycastOrigins.squeezedTop);
        }
    }

    private void DownwardMove(ref Vector2 deltaMovement)
    {
        float vertDistanceBetweenRays = doubleSqueezedVerticalDistanceBetweenRays;

        FindInnerRaycastOrigins();

        HorizontalSqueeze(vertDistanceBetweenRays, raycastOrigins.innerRight, raycastOrigins.innerLeft);

        VerticalSqueeze();


        FindOuterRaycastOrigins();
        Vector2 outerRightOrigin = raycastOrigins.squeezedRight;
        Vector2 outerLeftOrigin = raycastOrigins.squeezedLeft;

        CheckForWall(vertDistanceBetweenRays, outerRightOrigin, outerLeftOrigin);

        if (deltaMovement.x != 0f)
        {
            MoveHorizontally(ref deltaMovement, vertDistanceBetweenRays, outerRightOrigin, outerLeftOrigin);
        }

        if(deltaMovement.y != 0f)
        {
            MoveVertically(ref deltaMovement, horizontalDistanceBetweenRays, raycastOrigins.bottomLeft);
        }

        CheckForNearGround();
    }

    /*
     * Ray Color Legend
     * 
     * Wall
     *  Facing - Cyan
     *  Opposite - Magenta
     *  
     * Horizontal - Yellow
     * Vertical - Red
     * Near Ground - Green
     */
     
    /// <summary>
    /// 
    /// </summary>
    /// <param name="length"> The length of the raycasts </param>
    private void HorizontalSqueeze(float distanceBetweenRays, Vector2 rightOrigin, Vector2 leftOrigin)
    {
        bool rightSide = !spriteRenderer.flipX;
        float distance = squeezeDistance;
        Vector2 direction = rightSide ? Vector2.right : Vector2.left;
        Vector2 origin = rightSide ? rightOrigin : leftOrigin;

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < totalHorizontalRays; i++)
            {
                Vector2 start = new Vector2(origin.x, origin.y + (i * distanceBetweenRays));

                if (j == 0)
                    DrawRay(start, direction * distance, Color.green);
                else
                    DrawRay(start, direction * distance, Color.green);

                raycastHit = Physics2D.Raycast(start, direction, distance, platformMask);

                if (raycastHit)
                {
                    raycastHitsThisFrame.Add(raycastHit);

                    if (rightSide)
                    {
                        collisionState.rightSqueezed = true;
                    }
                    else
                    {
                        collisionState.leftSqueezed = true;
                    }

                    Vector3 deltaPosition = Vector3.zero;

                    float boxBoundX = rightSide ? boxCollider.bounds.max.x : boxCollider.bounds.min.x;

                    deltaPosition.x = raycastHit.point.x - boxBoundX;

                    transform.Translate(deltaPosition, Space.World);

                    break;
                }
            }

            if (collisionState.onWall == 0)
            {
                rightSide = !rightSide;
                direction *= -1;
                origin = rightSide ? rightOrigin : leftOrigin;
            }
            else
            {
                break;
            }
        }

        if (collisionState.rightSqueezed && collisionState.leftSqueezed)
        {
            Squeeze();
        }
    }

    private void VerticalSqueeze()
    {
        float distance = squeezeDistance;
        Vector2 direction = Vector2.down;
        Vector2 origin = raycastOrigins.innerBottom;

        for (int i = 0; i < totalVerticalRays; i++)
        {
            Vector2 start = new Vector2(origin.x + (i * horizontalDistanceBetweenRays), origin.y);

            DrawRay(start, direction * distance, Color.blue);

            raycastHit = Physics2D.Raycast(start, direction, distance, platformMask);

            if (raycastHit)
            {
                raycastHitsThisFrame.Add(raycastHit);

                collisionState.bottomSqueezed = true;

                Vector3 deltaPosition = Vector3.zero;

                deltaPosition.y = raycastHit.point.y - boxCollider.bounds.min.y;

                TruncateFloat(ref deltaPosition.y);

                transform.Translate(deltaPosition, Space.World);

                break;
            }
        }
    }

    private void CheckForWall(float distanceBetweenRays, Vector2 rightOrigin, Vector2 leftOrigin)
    {
        bool rightSide = !spriteRenderer.flipX;
        float distance = WallRaycastDistance + skinWidth;
        Vector2 direction = rightSide ? Vector2.right : Vector2.left;
        Vector2 origin = rightSide ? rightOrigin : leftOrigin;

        // This loop is incase we don't find a wall on the first side
        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < totalHorizontalRays; i++)
            {
                Vector2 start = new Vector2(origin.x, origin.y + (i * distanceBetweenRays));

                if (j == 0)
                    DrawRay(start, direction * distance, Color.cyan);
                else
                    DrawRay(start, direction * distance, Color.magenta);

                raycastHit = Physics2D.Raycast(start, direction, distance, platformMask);

                if (raycastHit)
                {
                    raycastHitsThisFrame.Add(raycastHit);

                    if (raycastHit.collider.tag == "Wall" || raycastHit.collider.tag == "Ridable")
                    {
                        collisionState.onWall = rightSide ? 1 : -1;

                        // found a wall break out
                        break;
                    }
                }
            }

            if (collisionState.onWall == 0)
            {
                rightSide = !rightSide;
                direction *= -1;
                origin = rightSide ? rightOrigin : leftOrigin;
            }
            else
            {
                break;
            }
        }
    }

    //Movement to the sides
    private void MoveHorizontally(ref Vector2 deltaMovement, float distanceBetweenRays, Vector2 rightOrigin, Vector2 leftOrigin)
    {
        bool rightSide = deltaMovement.x > 0f;
        float distance = Mathf.Abs(deltaMovement.x) + skinWidth;
        Vector2 direction = rightSide ? Vector2.right : Vector2.left;
        Vector2 origin = rightSide ? rightOrigin : leftOrigin;

        for (int i = 0; i < totalHorizontalRays; i++)
        {
            Vector2 start = new Vector2(origin.x, origin.y + (i * distanceBetweenRays));

            DrawRay(start, direction * distance, Color.yellow);

            raycastHit = Physics2D.Raycast(start, direction, distance, platformMask);

            if (raycastHit)
            {
                raycastHitsThisFrame.Add(raycastHit);

                //Shorten deltamovement to the point of collision
                deltaMovement.x = raycastHit.point.x - start.x;

                distance = Mathf.Abs(deltaMovement.x);

                TruncateFloat(ref deltaMovement.x);

                if (rightSide)
                {
                    deltaMovement.x -= skinWidth;
                }
                else
                {
                    deltaMovement.x += skinWidth;
                }

                if (Mathf.Abs(deltaMovement.x) < 0.0001f)
                {
                    deltaMovement.x = 0;
                }

                if (distance < skinWidth + 0.0001f)
                    break;
            }
        }
    }

    //Movement up and down
    private void MoveVertically(ref Vector2 deltaMovement, float distanceBetweenRays, Vector2 origin)
    {
        bool bottomSide = deltaMovement.y < 0f;
        float distance = Mathf.Abs(deltaMovement.y) + skinWidth;
        Vector2 direction = bottomSide ? Vector2.down : Vector2.up;

        for (int i = 0; i < totalVerticalRays; i++)
        {
            Vector2 start = new Vector2(origin.x + (i * distanceBetweenRays), origin.y);

            DrawRay(start, direction * distance, Color.red);

            raycastHit = Physics2D.Raycast(start, direction, distance, platformMask);

            if (raycastHit)
            {
                raycastHitsThisFrame.Add(raycastHit);

                if (raycastHit.collider.CompareTag("Ridable"))
                {
                    rider.Riding(raycastHit.collider.GetComponent<Ridable>());
                }

                //Shorten deltamovement to the point of collision
                deltaMovement.y = raycastHit.point.y - start.y;
                
                TruncateFloat(ref deltaMovement.y);

                if (bottomSide)
                {
                    deltaMovement.y += skinWidth;

                    collisionState.onGround = true;
                }
                else
                {
                    deltaMovement.y -= skinWidth;

                    collisionState.wallAbove = true;
                }

                break;
            }
        }
    }

    private void CheckForNearGround()
    {
        float distance = NearGroundRaycastDistance + skinWidth;
        Vector2 direction = Vector2.down;
        Vector2 origin = raycastOrigins.bottomLeft;

        for (int i = 0; i < totalVerticalRays; i++)
        {
            Vector2 start = new Vector2(origin.x + (i * horizontalDistanceBetweenRays), origin.y);
                
            DrawRay(start, direction * distance, Color.green);

            raycastHit = Physics2D.Raycast(start, direction, distance, platformMask);

            if (raycastHit)
            {
                raycastHitsThisFrame.Add(raycastHit);

                collisionState.nearGround = true;

                break;
            }
        }
    }

    private void TruncateFloat(ref float number)
    {
        number = Mathf.Round(number * 100f);
        number = number / 100f;
    }

    [System.Diagnostics.Conditional("DEBUG_RAYS")]
    void DrawRay(Vector3 start, Vector3 end, Color color)
    {
        Debug.DrawRay(start, end, color);
    }
}
