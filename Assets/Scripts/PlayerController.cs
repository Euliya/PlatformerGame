using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }
    FacingDirection direction;

    public float accTime;
    public float maxSpeed;
    public float decTime;

    public float ray;

    private float acc;
    private float dec;
    private Rigidbody2D rb;

    public float apexH;
    public float apexT;
    public float termSpeed;
    public float coyoteT;

    private float jumpGrv;
    private float grv;
    private float iniJumpV;
    private bool IsJump = false;
    [SerializeField]private bool IsCoyote = false;
    private bool onGround;

    public float dashT;
    private bool isDashing;

    public Vector2 playerInput;

    public LayerMask ground;

    bool isClimb=false;
    bool canClimb;
    bool isLeavingClimb;
    bool isClimbLastFrame;

    public float climbSpeed;

    public PhysicsMaterial2D BounceMaterial;

    // Start is called before the first frame update
    void Start()
    {
        acc = maxSpeed / accTime;
        dec = maxSpeed / accTime;
        rb = GetComponent<Rigidbody2D>();

        grv = rb.gravityScale;
        jumpGrv = -2 * apexH / Mathf.Pow(apexT, 2);
        iniJumpV = 2 * apexH / apexT;

        onGround = IsGrounded();

    }

    // Update is called once per frame
    void Update()
    {
        rb.sharedMaterial=mt();
        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.
        playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        MovementUpdate(playerInput);
        
    }

    
    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 currentV = rb.velocity;

        if (isDashing )
        {
            currentV.x = 4 * maxSpeed * playerInput.x;
        }
        else if (playerInput != Vector2.zero)
        {
            direction = playerInput.x < 0 ? FacingDirection.left : FacingDirection.right;
            currentV.x += acc * playerInput.x * Time.deltaTime;
            currentV.x = Mathf.Clamp(currentV.x, -maxSpeed, maxSpeed);
        }
        else if (Mathf.Abs(currentV.x )> 0)
        {
            currentV.x = currentV.normalized.x * Mathf.Clamp(Mathf.Abs(currentV.x) - dec * Time.deltaTime, 0, maxSpeed);
        }
        
        if (!IsGrounded() && onGround)
        {
            StartCoroutine(coyoteTime());
        }
        

        if (Input.GetButton("Jump") && (IsGrounded()||IsCoyote) && !isClimb)
        {
            IsJump = true;
            IsCoyote = false;
            rb.gravityScale = 0;
            currentV.y = iniJumpV;
            StartCoroutine(Falling());            
        }
        if (IsJump)
        {
            currentV.y += jumpGrv * Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isDashing = true;
            StartCoroutine(Dash());
        }

        
        if (playerInput.y != 0 && !isClimb && canClimb)
        {
            isClimb = true;
            rb.gravityScale = 0;
        }
        if (isClimb && canClimb)
        {
            currentV.y = climbSpeed * playerInput.normalized.y;
        }
        else
        {
            rb.gravityScale = grv;
        }

        if (onGround)
        {
            isClimb = false;
        }
        if (currentV.y>0 && !isClimb && isClimbLastFrame)
        {
            isLeavingClimb = true;
            print("leave");
        }

        if (isLeavingClimb)
        {
            currentV.y = climbSpeed * playerInput.normalized.y;
            if (!IsGrounded() && onGround)
            {
                currentV = Vector2.zero;
                isLeavingClimb = false;
                rb.gravityScale = grv;
            }
        }

        isClimbLastFrame = isClimb;
        onGround = IsGrounded();


        currentV.y = Mathf.Clamp(currentV.y, -termSpeed, iniJumpV);

        rb.velocity = currentV;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        canClimb = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        canClimb = false;
    }
    PhysicsMaterial2D mt()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, ray, ground);
        return hit.collider != null && hit.collider.CompareTag("Bounce") ? BounceMaterial : null;

    }
    IEnumerator Falling()
    {
        yield return new WaitForSeconds(apexT);
        rb.gravityScale = grv;
        IsJump = false;
    }
    IEnumerator coyoteTime()
    {
        IsCoyote = true;
        yield return new WaitForSeconds(coyoteT);
        IsCoyote = false;
    }
    IEnumerator Dash()
    {
        yield return new WaitForSeconds(dashT);
        isDashing = false;
    }
    public bool IsWalking()
    {
        return rb.velocity.x != 0;
    }
    public bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector2.down * ray, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, ray,ground);
        return hit.collider != null;
    }
    public bool IsClimbing()
    {
        return isClimb; 
    }
    public FacingDirection GetFacingDirection()
    {
        return direction;
    }
}
