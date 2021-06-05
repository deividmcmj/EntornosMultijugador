using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class PlayerController : NetworkBehaviour
{
    #region Variables

    [Header("Movement")] public List<AxleInfo> axleInfos;
    public float forwardMotorTorque = 100000;
    public float backwardMotorTorque = 50000;
    public float maxSteeringAngle = 15;
    public float engineBrake = 1e+12f;
    public float footBrake = 1e+24f;
    public float topSpeed = 200f;
    public float downForce = 100f;
    public float slipLimit = 0.2f;

    private float CurrentRotation { get; set; }
    private float InputAcceleration { get; set; }
    private float InputSteering { get; set; }
    private float InputBrake { get; set; }

    //Devuelve true si el jugador se puede mover y false si no.
    public bool CanMove { get; set; }

    //Devuelve true si el coche está boca abajo y false si no.
    private bool UpsideDown
    {
        get
        {
            return Mathf.Abs(transform.rotation.eulerAngles.z) >= 50f && Mathf.Abs(transform.rotation.eulerAngles.z) <= 310f;
        }
    }
    //Devuelve true si el coche está parado y false si no.
    private bool Stopped
    {
        get
        {
            return m_Rigidbody.velocity.magnitude <= 0.01f;
        }
    }

    private PlayerInfo m_PlayerInfo;

    private Rigidbody m_Rigidbody;
    private SetupPlayer m_SetupPlayer;
    private float m_SteerHelper = 0.8f;
    //Tiempo que se está yendo en sentido contrario
    private float m_WrongDirectionTimer = 0.0f;

    private float m_CurrentSpeed = 0;

    private float Speed
    {
        get { return m_CurrentSpeed; }
        set
        {
            if (Math.Abs(m_CurrentSpeed - value) < float.Epsilon) return;
            m_CurrentSpeed = value;
            if (OnSpeedChangeEvent != null)
                OnSpeedChangeEvent(m_CurrentSpeed);
        }
    }

    public event Action<float> OnSpeedChangeEvent;

    private Vector2 _movement;
    private InputController _input;

    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    //public delegate void OnSpeedChangeDelegate(float newVal);

    //public event OnSpeedChangeDelegate OnSpeedChangeEvent;

    #endregion Variables

    #region Unity Callbacks

    public void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_PlayerInfo = GetComponent<PlayerInfo>();
        m_SetupPlayer = GetComponent<SetupPlayer>();
        CanMove = false;

        _input = new InputController();
    }

    public void Update()
    {
        _movement = _input.Player.Movement.ReadValue<Vector2>();

        InputAcceleration = _movement.y;
        InputSteering =_movement.x;

        /*
        InputAcceleration = Input.GetAxis("Vertical");
        InputSteering = Input.GetAxis(("Horizontal"));
        InputBrake = Input.GetAxis("Jump");
        */
        Speed = m_Rigidbody.velocity.magnitude;
    }

    public void FixedUpdate()
    {
        if (!CanMove)
        {
            InputSteering = 0f;
            InputAcceleration = 0f;
            InputBrake = 0f;
        }
        else
        {
            InputSteering = Mathf.Clamp(InputSteering, -1, 1);
            InputAcceleration = Mathf.Clamp(InputAcceleration, -1, 1);
            InputBrake = Mathf.Clamp(InputBrake, 0, 1);
        }

        float steering = maxSteeringAngle * InputSteering;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }

            if (axleInfo.motor)
            {
                if (InputAcceleration > float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = forwardMotorTorque;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = forwardMotorTorque;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (InputAcceleration < -float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = -backwardMotorTorque;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = -backwardMotorTorque;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (Math.Abs(InputAcceleration) < float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = 0;
                    axleInfo.leftWheel.brakeTorque = engineBrake;
                    axleInfo.rightWheel.motorTorque = 0;
                    axleInfo.rightWheel.brakeTorque = engineBrake;
                }

                if (InputBrake > 0)
                {
                    axleInfo.leftWheel.brakeTorque = footBrake;
                    axleInfo.rightWheel.brakeTorque = footBrake;
                }
            }

            //Si el jugador está boca abajo y parado, se vuelve a colocar en la pista
            if (UpsideDown && Stopped)
            {
                DisablePhysics();
                transform.position = new Vector3(m_PlayerInfo.CurrentCircuitPosition.x, 0.51f, m_PlayerInfo.CurrentCircuitPosition.z);
                transform.rotation = Quaternion.LookRotation((m_PlayerInfo.LookAtPoint - m_PlayerInfo.CurrentCircuitPosition), Vector3.up);
                EnablePhysics();
            }

            //Si el jugador está yendo en sentido contrario, se muestra un mensaje hasta que vuelva a seguir correctamente
            if (!Stopped && m_PlayerInfo.Direction < 0)
            {
                m_PlayerInfo.Backwards = true;
                m_WrongDirectionTimer += Time.deltaTime;
                if (m_WrongDirectionTimer >= 2.0f)
                {
                    m_SetupPlayer.UpdateDirectionMessage("Dirección incorrecta");
                }
            }
            else if (!Stopped && m_PlayerInfo.Direction > 0 && m_WrongDirectionTimer > 0.0f)
            {
                m_PlayerInfo.Backwards = false;
                m_WrongDirectionTimer = 0.0f;
                m_SetupPlayer.UpdateDirectionMessage("");
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

        SteerHelper();
        SpeedLimiter();
        AddDownForce();
        TractionControl();

        m_PlayerInfo.Speed = m_Rigidbody.velocity;
    }

    #endregion

    #region Methods

    //Desactiva las físicas del jugador
    public void DisablePhysics()
    {
        m_Rigidbody.isKinematic = true;
        m_Rigidbody.detectCollisions = false;
    }
    //Activa las físicas del jugador
    public void EnablePhysics()
    {
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.detectCollisions = true;
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit wheelHitLeft;
            WheelHit wheelHitRight;
            axleInfo.leftWheel.GetGroundHit(out wheelHitLeft);
            axleInfo.rightWheel.GetGroundHit(out wheelHitRight);

            if (wheelHitLeft.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitLeft.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.leftWheel.motorTorque -= axleInfo.leftWheel.motorTorque * howMuchSlip * slipLimit;
            }

            if (wheelHitRight.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitRight.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.rightWheel.motorTorque -= axleInfo.rightWheel.motorTorque * howMuchSlip * slipLimit;
            }
        }
    }

// this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        foreach (var axleInfo in axleInfos)
        {
            axleInfo.leftWheel.attachedRigidbody.AddForce(
                -transform.up * (downForce * axleInfo.leftWheel.attachedRigidbody.velocity.magnitude));
        }
    }

    private void SpeedLimiter()
    {
        float speed = m_Rigidbody.velocity.magnitude;
        if (speed > topSpeed)
            m_Rigidbody.velocity = topSpeed * m_Rigidbody.velocity.normalized;
    }

// finds the corresponding visual wheel
// correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider col)
    {
        if (col.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = col.transform.GetChild(0);
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);
        var myTransform = visualWheel.transform;
        myTransform.position = position;
        myTransform.rotation = rotation;
    }

    private void SteerHelper()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit[] wheelHit = new WheelHit[2];
            axleInfo.leftWheel.GetGroundHit(out wheelHit[0]);
            axleInfo.rightWheel.GetGroundHit(out wheelHit[1]);
            foreach (var wh in wheelHit)
            {
                if (wh.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }
        }

// this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(CurrentRotation - transform.eulerAngles.y) < 10f)
        {
            var turnAdjust = (transform.eulerAngles.y - CurrentRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }

        CurrentRotation = transform.eulerAngles.y;
    }

    #endregion
}