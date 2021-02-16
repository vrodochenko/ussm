using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerBehaviour : MonoBehaviour
{

    public float acceleration;
    private Rigidbody rb;

    private float rotateYaw; // рысканье
    private float rotateRoll; // крен
    private float rotatePitch; // тангаж

    public float yawSpeed; //скорость рысканья, от 0 до 1 по задумке
    public float rollSpeed; // скорость крена, от 0 до 1 по задумке
    public float pitchSpeed; //скорость тангажа, от 0 до 1 по задумке

    public float maxSpeed;
    public Vector3 currentSpeed; // текущая скорость корабля
    public Vector3 moveThrottle; //текущее ускорение корабля
    public bool movingBack;

    private Transform startTransform;
    private Vector3 initialPosition;
    [SerializeField] private Quaternion initialRotation;
    [SerializeField] private Quaternion currentRotation;

    [SerializeField] private Vector3 currentRelativeRotation;

    public bool isStabilizingRoll;
    public bool isStabilizingPitch;

    public float rollTreshold;

    void Start()
    {
        startTransform = this.gameObject.transform;
        initialPosition = startTransform.position;
        initialRotation = startTransform.rotation;
        rb = this.gameObject.GetComponent<Rigidbody>();

        isStabilizingRoll = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude * transform.forward;
        currentRotation = startTransform.rotation;
        currentRelativeRotation = Vector3.Cross(transform.up, Vector3.up);
        TranslateShip();
        //if(!isStabilizingPitch && !isStabilizingRoll)
       // {
            RotateShip();
       // }
        StopOnButton();
        //StopOnZeroAcceleration();
        ReverseMoveDirection();
        StabilizeRoll();
        StabilizePitch();
    }

    void StopOnButton()
    {
        if (CrossPlatformInputManager.GetButtonDown("stopButton"))
        {
            StopForcesOnPlayer();
            rb.velocity = 0.01f * rb.velocity;
        }
    }

    void ReverseMoveDirection()
    {
        if (CrossPlatformInputManager.GetButtonDown("reverseGear"))
        {
            movingBack = !movingBack;
        }
    }

    void StopOnZeroAcceleration()
    {
        // остановка игрока при достижении ускорением минимального значения (можно -1)
        if (acceleration == 0)
        {
            StopForcesOnPlayer();
            rb.velocity = 0.01f * rb.velocity;
        }
    }

    public void SetAcceleration(float new_acceleration)
    // функция, которая цепляется к событию OnValueChanged слайдера
    {
        acceleration = new_acceleration;
    }

    void TranslateShip()
    {
        Vector3 Forward = transform.forward;
        moveThrottle = Vector3.zero;

        //перемещаем 
        if (!movingBack)
            moveThrottle += Forward;
        else
            moveThrottle -= Forward;

        moveThrottle *= acceleration;
        rb.AddForce(moveThrottle, ForceMode.VelocityChange);
        rb.velocity = new Vector3(ClampVelocity(rb.velocity.x), ClampVelocity(rb.velocity.y), ClampVelocity(rb.velocity.z));
    }

    private float ClampVelocity(float a) // делаем так, чтобы величина осталась в заданных пределах. Применим потом к скорости по каждой из осей
    {
        return Mathf.Clamp(a, -maxSpeed, maxSpeed);
    }

    void RotateShip()
    {
        // Smoothly tilts a transform towards a target rotation.
        rotateYaw = CrossPlatformInputManager.GetAxis("yaw");
        rotateRoll = CrossPlatformInputManager.GetAxis("roll");
        rotatePitch = CrossPlatformInputManager.GetAxis("pitch");

        rb.AddTorque(transform.up * yawSpeed * rotateYaw);  // рысканье
        rb.AddTorque(transform.forward * rollSpeed * rotateRoll); // крен
        rb.AddTorque(transform.right * pitchSpeed* rotatePitch);  // тангаж (кивок)
    }

    public void StopForcesOnPlayer()
    {

        // will shiver othervise
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void StabilizeRoll()
    {
        if (CrossPlatformInputManager.GetButtonDown("rollStabilization"))
        {
            isStabilizingRoll = true;
            StopForcesOnPlayer();
            StartCoroutine("ResetRoll");
        }
        if (isStabilizingRoll && Mathf.Abs(currentRelativeRotation.z) < rollTreshold)
        {
            StopForcesOnPlayer();
            StopCoroutine("ResetRoll");
            isStabilizingRoll = false;
        }
    }

    IEnumerator ResetRoll()
    {
        // inspired by https://forum.unity.com/threads/rotate-rigidbody-with-addtorque-towards-a-specific-location.898046/
        if (Mathf.Abs(currentRelativeRotation.z) > rollTreshold)
        {
            rb.AddTorque(50 * Vector3.Project(currentRelativeRotation, rb.transform.forward));
            yield return null;
        }

    }

    void StabilizePitch()
    {
        if (CrossPlatformInputManager.GetButtonDown("pitchStabilization"))
        {
            isStabilizingPitch = true;
            StopForcesOnPlayer();
            StartCoroutine("ResetPitch");
        }
        if (isStabilizingPitch && Mathf.Abs(currentRelativeRotation.x) < rollTreshold)
        {
            StopForcesOnPlayer();
            StopCoroutine("ResetPitch");
            isStabilizingPitch = false;
        }
    }

    IEnumerator ResetPitch()
    {
        // inspired by https://forum.unity.com/threads/rotate-rigidbody-with-addtorque-towards-a-specific-location.898046/
        if (Mathf.Abs(currentRelativeRotation.x) > rollTreshold)
        {
            rb.AddTorque(50 * Vector3.Project(currentRelativeRotation, rb.transform.right));
            yield return null;
        }

    }

 
}
