using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Vehicle : Destructible
{

    [SerializeField]
    protected float maxLinearVelocity;


    [Header("EngineSound")]
    [SerializeField]
    private AudioSource engineSound;

    [SerializeField]
    private float enginePitchMOdifier;

    [Header("Vehicle")]
    [SerializeField] protected Transform zoomOpticsPosition;
    public Transform ZoomOpticsPosition => zoomOpticsPosition;

    public virtual float LinearVelocity => 0;

    public float NormalizedLinearVelocity
    {
        get
        {
            if (Mathf.Approximately(0,LinearVelocity ) == true)  return 0;

            return Mathf.Clamp01(LinearVelocity / maxLinearVelocity);
        }
    }

    [SyncVar]
    private Vector3 netAimPoint;

    public Vector3 NetAimPoint
    {
        get => netAimPoint;

        set
        {
            netAimPoint = value;    // client
            CmdSetNetAimPoint(value); // server
        }
    }

    [Command]
    private void CmdSetNetAimPoint(Vector3 v)
    {
        netAimPoint = v;
    }



    protected Vector3 targetInputControl;

    public void SetTargetControl(Vector3 control)
    {
        targetInputControl = control.normalized;
    }

 

    protected virtual void Update()
    {
        UpdateEngineSFX();
    }

    private void UpdateEngineSFX()
    {
        if (engineSound != null)
        {
            engineSound.pitch = 1.0f + enginePitchMOdifier * NormalizedLinearVelocity;
            engineSound.volume = 0.5f + NormalizedLinearVelocity;
        }
    }


}
