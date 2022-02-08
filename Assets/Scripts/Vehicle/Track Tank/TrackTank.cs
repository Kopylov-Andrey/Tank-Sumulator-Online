using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class TrackWheelRow
{
    [SerializeField]
    private WheelCollider[] colliders;

    [SerializeField]
    private Transform[] meshs;

    public float minRpm;

    public void SetTorque(float motorTorque)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].motorTorque = motorTorque;
        }
        Debug.Log(motorTorque);
    }

    public void Break(float brakeTorque)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].brakeTorque = brakeTorque;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].motorTorque = 0;
            colliders[i].brakeTorque = 0;
        }
    }

    public void SetSidewayStiffnesss(float stiffness)
    {
         WheelFrictionCurve wheelFrictionCurve = new WheelFrictionCurve();

        for (int i = 0; i < colliders.Length; i++)
        {
            wheelFrictionCurve = colliders[i].sidewaysFriction;
            wheelFrictionCurve.stiffness = stiffness;

            colliders[i].sidewaysFriction = wheelFrictionCurve;
        }
    }

    public void UppdateMAshTrasform()
    {
        //Find min rpm

        List<float> allRpm = new List<float>(); 

        for (int i = 0;i < colliders.Length; i++)
        {
            if (colliders[i].isGrounded == true)
            {
                allRpm.Add(colliders[i].rpm);
            }
        }

        if (allRpm.Count > 0)
        {
            minRpm = Mathf.Abs( allRpm[0]);
            for (int i = 0; i < allRpm.Count; i++)
            {
                if(Mathf.Abs(allRpm[i]) < minRpm)
                {
                    minRpm = Mathf.Abs(allRpm[i]);
                }
            }
            minRpm = minRpm * Mathf.Sign(allRpm[0]);    
        }

        float angle = minRpm * 360.0f / 60.0f * Time.deltaTime;


        for (int i = 0; i < meshs.Length; i++)
        {
            Vector3 position;
            Quaternion rotation;

            colliders[i].GetWorldPose(out position, out rotation);

            meshs[i].position = position;
            meshs[i].Rotate(angle, 0, 0);   
        }
    }

    public void UpdateMeshRotationByRpm(float rpm)
    {
        float angle = rpm * 360.0f /60.0f * Time.fixedDeltaTime;


        for (int i = 0; i < meshs.Length; i++)
        {
            Vector3 position;
            Quaternion rotation;

            colliders[i].GetWorldPose(out position, out rotation);

            meshs[i].position = position;
            meshs[i].Rotate(angle, 0, 0);
        }
    }
}


[RequireComponent(typeof(Rigidbody))]
public class TrackTank : Vehicle
{

    public override float LinearVelocity => rigidbody.velocity.magnitude;

    [SerializeField] private Transform centerOfMass;

    [Header("Tracks")]
    [SerializeField] private TrackWheelRow leftWheelRow;
    [SerializeField] private TrackWheelRow rightWheelRow;


    [Header("Movement")]
    [SerializeField] private ParameterCurve forwardTorqueCurve;     // кривая жвижения вперед
    [SerializeField] private float maxForvardTorque;                // крутяший момент
    [SerializeField] private float maxBackwardMotorTorque;
    [SerializeField] private ParameterCurve backwardTorqueCurve;    // кривая жвижения назад
    [SerializeField] private float breakTorque;                     // тормозное усилие
    [SerializeField] private float rollingResistance;               // тормозное усилие (если не нажимать ничего) 


    [Header("Rotation")]
    [SerializeField] private float rotateTorqueInPlace;             // усилие крутящего момента на месте  
    [SerializeField] private float rotateBreakInPlace;              // тормозное усилие на месте  
    [Space(2)]
    [SerializeField] private float rotateTorqueInMotion;            // усилие крутящего момента в движении  
    [SerializeField] private float rotateBreakInMotion;             // тормозное усилие в движении
                                                             
    [Header("Friction")]
    [SerializeField] private float minSidewayStiffnessInPlace;      // мнимальная сила бокового трения на месте
    [SerializeField] private float minSidewayStiffnessInMotion;     // мнимальная сила бокового трения в движении


    public float LeftWheelRpm => leftWheelRow.minRpm;

    public float RightWheelRpm => rightWheelRow.minRpm;



    private /*new*/ Rigidbody  rigidbody;
    [SerializeField] private float currentMotorTorque;


    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centerOfMass.localPosition;
    }

    private void FixedUpdate()
    {
      


        if (hasAuthority == true)
        {
            UpdateMotorTorque();

            CmdUpdateWheelRpm(LeftWheelRpm, RightWheelRpm);
        }

    }

    [Command]
    private void CmdUpdateWheelRpm(float leftRpm, float rightRpm)
    {
        SvUpdateWheelRpm(leftRpm, rightRpm);
    }


    [Server]
    private void SvUpdateWheelRpm(float leftRpm, float rightRpm)
    {
        RpcUpdateWheelRpm(leftRpm, leftRpm);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcUpdateWheelRpm(float leftRpm, float rightRpm)
    {
        leftWheelRow.minRpm = leftRpm;  
        rightWheelRow.minRpm = rightRpm;

        leftWheelRow.UpdateMeshRotationByRpm(leftRpm);
        rightWheelRow.UpdateMeshRotationByRpm(leftRpm);
    }


    private void UpdateMotorTorque()
    {
        float targetMotorTorqe = targetInputControl.z > 0 ? maxForvardTorque * Mathf.RoundToInt(targetInputControl.z) : maxBackwardMotorTorque * Mathf.RoundToInt(targetInputControl.z); // движение
        float breakTorque = this.breakTorque * targetInputControl.y; // тормоз
        float steering = targetInputControl.x; // поворот

        #region UPDATE TARGET MOTOR TORQUE

        if (targetMotorTorqe > 0)
        {
            currentMotorTorque = forwardTorqueCurve.MoveTowards(Time.fixedDeltaTime) * targetMotorTorqe;
        }

        if (targetMotorTorqe < 0)
        {
            currentMotorTorque = backwardTorqueCurve.MoveTowards(Time.fixedDeltaTime) * targetMotorTorqe;
        }

        if (targetMotorTorqe == 0)
        {
            currentMotorTorque = forwardTorqueCurve.Reset();
            currentMotorTorque = backwardTorqueCurve.Reset();
        }

        #endregion


        #region BREAK

        leftWheelRow.Break(breakTorque);
        rightWheelRow.Break(breakTorque);

        #endregion


        #region ROLLING
        if (targetMotorTorqe == 0 && steering == 0)
        {
            leftWheelRow.Break(rollingResistance);
            rightWheelRow.Break(rollingResistance);
        }
        else
        {
            leftWheelRow.Reset();
            rightWheelRow.Reset();
        }


        #endregion


        #region ROTATE IN PLACE


        if (targetMotorTorqe == 0 && steering != 0)
        {
            if (Mathf.Abs(leftWheelRow.minRpm) < 1 || Mathf.Abs(rightWheelRow.minRpm) < 1)
            {
                leftWheelRow.SetTorque(rotateTorqueInPlace);
                rightWheelRow.SetTorque(rotateTorqueInPlace);
            }
            else
            {
                if (steering < 0)
                {
                    leftWheelRow.Break(rotateBreakInPlace);
                    rightWheelRow.SetTorque(rotateTorqueInPlace);
                }
                if (steering > 0)
                {
                    leftWheelRow.SetTorque(rotateTorqueInPlace);
                    rightWheelRow.Break(rotateBreakInPlace);
                }
            }



            leftWheelRow.SetSidewayStiffnesss(1.0f + minSidewayStiffnessInPlace - Mathf.Abs(steering));
            rightWheelRow.SetSidewayStiffnesss(1.0f + minSidewayStiffnessInPlace - Mathf.Abs(steering));
        }


        #endregion

        #region MOVE
        if (targetMotorTorqe != 0)
        {
            if (steering == 0)
            {
                if (LinearVelocity < maxLinearVelocity)
                {
                    leftWheelRow.SetTorque(currentMotorTorque);
                    rightWheelRow.SetTorque(currentMotorTorque);

                }
            }

            if (steering != 0 && (Mathf.Abs(leftWheelRow.minRpm) < 1 || Mathf.Abs(rightWheelRow.minRpm) < 1))
            {
                leftWheelRow.SetTorque(rotateTorqueInMotion);
                rightWheelRow.SetTorque(rotateTorqueInMotion);
            }
            else
            {
                if (steering < 0)
                {
                    leftWheelRow.Break(rotateBreakInMotion);
                    rightWheelRow.SetTorque(rotateTorqueInMotion);
                }
                if (steering > 0)
                {
                    leftWheelRow.SetTorque(rotateTorqueInMotion);
                    rightWheelRow.Break(rotateBreakInMotion);
                }
            }

            leftWheelRow.SetSidewayStiffnesss(1.0f + minSidewayStiffnessInMotion - Mathf.Abs(steering));
            rightWheelRow.SetSidewayStiffnesss(1.0f + minSidewayStiffnessInMotion - Mathf.Abs(steering));

        }


        #endregion

        leftWheelRow.UppdateMAshTrasform();
        rightWheelRow.UppdateMAshTrasform();
    }
}

