using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour {
    struct FrameInput {
        public Vector2 MoveInput { get; set; }
        public bool Jumping { get; set; }
    }

    private FrameInput frameInput = new FrameInput();

    [Header("General Movement Settings")]
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpForce;

    public delegate void PlayerLand();
    public event PlayerLand OnPlayerSmoke;

    public float Magnitude { get; private set; }
    public Vector3 Velocity { get; private set; }
    public float MagToMaxRatio => Mathf.Clamp01(Magnitude / (GetMaxSpeed() * 1.5f));

    [Header("Hover Settings")]
    [SerializeField] private LayerMask environment;
    [SerializeField] private LayerMask vaultEnvironment;
    [SerializeField] private float vaultHeight;
    [SerializeField] private float rideHeight;
    [SerializeField] private float rideSpringStrength;
    [SerializeField] private float rideSpringDamper;
    [SerializeField] private float uprightSpringStrength;
    private bool grounded = false;
    public bool Grounded {
        get => grounded;

        private set {
            if (grounded != value && value) OnPlayerLand?.Invoke(Mathf.Abs(Velocity.y * 0.4f));

            grounded = value;
        }
    }

    public event PlayerInput.ReceieveFloatInput OnPlayerLand;

    [Header("Friction Settings")]
    [SerializeField] private float friction;
    [SerializeField] private float frictionMultiplier;
    [SerializeField] private int counterThresold;
    private int readyToCounter = 0;

    [Header("Swimming Settings")]
    [SerializeField] private float swimJumpBoost;
    public float Submergence { get; set; } = 0f;

    [Header("Refrences")]
    [SerializeField] private PhysicMaterial bouncy;
    [SerializeField] private PhysicMaterial slippery;
    [SerializeField] private PlayerRef player;
    [SerializeField] private Rigidbody2D rb;
    public Rigidbody2D Rb => rb;

    void Awake() {
        player.PlayerInput.OnMoveInput += (Vector2 v) => frameInput.MoveInput = v;
        player.PlayerInput.OnJumpHoldInput += ReceiveJumpInput;
    }

    void FixedUpdate() {
        ApplyFriction();
        HoverOffGround(CalculateVault());
        UpdateUprightForce();

        Magnitude = rb.velocity.magnitude;
        Velocity = rb.velocity;

        float movementMultiplier = 3.5f * Time.fixedDeltaTime * (Grounded ? 1f : 0.6f);
        ClampSpeed(movementMultiplier);

        rb.AddForce(acceleration * movementMultiplier * frameInput.MoveInput.x * Vector3.right, ForceMode2D.Impulse);
    }

    private void ReceiveJumpInput(bool jumping) {
        frameInput.Jumping = jumping;

        if (!jumping || !grounded) return;

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(jumpForce * Vector3.up, ForceMode2D.Impulse);

        OnPlayerSmoke?.Invoke();
    }

    private void ClampSpeed(float movementMultiplier) {
        float horizVel = Velocity.x;
        float maxSpeed = GetMaxSpeed();
        float coefficientOfFriction = acceleration / maxSpeed;

        if (Mathf.Abs(horizVel) > maxSpeed) {
            OnPlayerSmoke?.Invoke();
            rb.AddForce(coefficientOfFriction * movementMultiplier * -horizVel * Vector3.right, ForceMode2D.Impulse);
        }
    }

    private void ApplyFriction() {
        if (frameInput.Jumping || !Grounded) return;

        static bool CounterMomentum(float input, float mag, float threshold = 0.05f)
        => input > 0 && mag < -threshold || input < 0 && mag > threshold;

        Vector3 frictionForce = Vector3.zero;

        if (Mathf.Abs(Velocity.x) > 0f && frameInput.MoveInput.x == 0f && readyToCounter > counterThresold) 
            frictionForce -= Vector3.right * Velocity.x;
        if (CounterMomentum(frameInput.MoveInput.x, Velocity.x)) 
            frictionForce -= Vector3.right * Velocity.x;

        frictionForce = Vector3.ProjectOnPlane(frictionForce, Vector3.up);
        if (frictionForce != Vector3.zero) rb.AddForce(0.2f * friction * acceleration * frictionForce);        

        readyToCounter = frameInput.MoveInput.x == 0f ? readyToCounter + 1 : 0;
    }

    private float CalculateVault() {
        float playerHeight = player.CapsuleCol.size.y;
        Vector3 pos = transform.position + (Vector3.right * frameInput.MoveInput.x) + (0.5f * playerHeight * Vector3.down);
        bool wallCheck = Physics2D.Raycast(pos + Vector3.up * vaultHeight, Vector3.down, 0.5f * playerHeight, vaultEnvironment);

        //-Debug- Debug.DrawRay(pos + Vector3.up * vaultHeight, Vector3.down * 0.5f * playerHeight, wallCheck ? Color.green : Color.red);

        return wallCheck ? frameInput.MoveInput.x : 0;
    }

    private void HoverOffGround(float bufferOffset) {
        float playerHeight = player.CapsuleCol.size.y;
        Vector3 bufferPosition = transform.position + (Vector3.right * bufferOffset) + (0.5f * playerHeight * Vector3.down);

        RaycastHit2D hit = Physics2D.Raycast(bufferPosition, Vector3.down, rideHeight, environment);
        bool buffer = hit.collider != null;

        Grounded = Physics2D.CircleCast(transform.position, 0.3f, Vector3.down, rideHeight, environment) || buffer;

        if (!buffer) return;

        Vector2 vel = rb.velocity;
        Vector2 otherVel = Vector2.zero;
        Rigidbody2D hitBody = hit.collider.GetComponent<Rigidbody2D>();

        if (hitBody != null) otherVel = hitBody.velocity;

        float relVel = Vector3.Dot(Vector3.down, vel) - Vector3.Dot(Vector3.down, otherVel);
        float x = hit.distance - rideHeight;
        float springForce = (x * rideSpringStrength) - (relVel * rideSpringDamper);

        rb.AddForce(Vector3.down * springForce);

        if (hitBody != null) hitBody.AddForceAtPosition(Vector3.down * -springForce, hit.point);
    }

    public void UpdateUprightForce() {
        rb.AddTorque(-rb.rotation * uprightSpringStrength * Time.fixedDeltaTime, ForceMode2D.Impulse);
    }

    private float GetMaxSpeed() {
        return maxSpeed * (Grounded ? 1f : 1.15f);
    }

}
