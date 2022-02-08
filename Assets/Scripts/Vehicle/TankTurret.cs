using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankTurret))]
public class TankTurret : MonoBehaviour
{
    private TrackTank tank;

    [SerializeField] private Transform tower;
    [SerializeField] private Transform mask;


    [SerializeField] private float horizontalRotationSpeed;
    [SerializeField] private float verticalRotationSpeed;


    [SerializeField] private float maxTopAngle;
    [SerializeField] private float maxBottomAngle;

    [Header("SFX")]
    [SerializeField] private AudioSource fireSound;
    [SerializeField] private ParticleSystem mazzel;
    [SerializeField] private float forceRecoil;


    float maskCurrentAngle;

    private Rigidbody tankRigitBody;
    private void Start()
    {
        tank = GetComponent<TrackTank>();
        tankRigitBody = tank.GetComponent<Rigidbody>();    
        maxTopAngle = -maxTopAngle;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        ControlTurretAim();
    }

    public void Fire()
    {
        FireSfx();
    }

    private void FireSfx()
    {
        fireSound.Play();
        mazzel.Play();

        tankRigitBody.AddForceAtPosition(-mask.forward * forceRecoil, mask.position, ForceMode.Impulse);
    }

    private void ControlTurretAim()
    {
        #region TOWER
        Vector3 localPosition = tower.InverseTransformPoint(tank.NetAimPoint);
        localPosition.y = 0;
        Vector3 globalPosition = tower.TransformPoint(localPosition);

        tower.rotation = Quaternion.RotateTowards(tower.rotation, Quaternion.LookRotation((globalPosition - tower.position).normalized, tower.up), horizontalRotationSpeed * Time.deltaTime);


        #endregion

        #region MASK
        mask.localRotation = Quaternion.identity;

        localPosition = mask.InverseTransformPoint(tank.NetAimPoint);
        localPosition.x = 0;
        globalPosition = mask.TransformPoint(localPosition);


        float targetAngl = -Vector3.SignedAngle((globalPosition - mask.position).normalized, mask.forward, mask.right);
        targetAngl = Mathf.Clamp(targetAngl, maxTopAngle, maxBottomAngle);
        maskCurrentAngle = Mathf.MoveTowards(maskCurrentAngle, targetAngl, Time.deltaTime * verticalRotationSpeed);
        mask.localRotation = Quaternion.Euler(maskCurrentAngle, 0, 0);
        #endregion
    }


}
