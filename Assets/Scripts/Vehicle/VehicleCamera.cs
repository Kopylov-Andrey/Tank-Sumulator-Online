using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class VehicleCamera : MonoBehaviour
{

    public static VehicleCamera Instance;

    [SerializeField] private Vehicle vehicle;
    [SerializeField] private Vector3 Offset;


    [Header("Sensetive Limit")]
    [SerializeField] private float rotateSencetive;
    [SerializeField] private float scrollSencetive;


    [Header("Rotation Limit")]
    [SerializeField] private float maxVerticleAngle;
    [SerializeField] private float minVerticleAngle;

    
    [Header("Distance")]
    [SerializeField] private float distance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float distanceOffsetFromCollisionHit;
    [SerializeField] private float distanceLerpRate;


    [Header("Zoom Options")]
    [SerializeField] private GameObject zoomMaskEffect;   
    [SerializeField] private float zoomedFov;
    [SerializeField] private float zoomedMaxVerticalAngle;


    private new Camera camera;


    private Vector2 rotationControl;

    private float defaultFov;
    private float deltaRotationX;
    private float deltaRotationY;
    private float currentDistance;
    private float defaultMaxVerticaLAngle;
    private float lastDistance;


    private bool isZoom;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this; 

    }



    private void Start()
    {
        camera  = GetComponent<Camera>();
        defaultFov = camera.fieldOfView;
        defaultMaxVerticaLAngle = maxVerticleAngle;

        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {

        if (vehicle == null) return;
        

        UpdateControle();
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        isZoom = distance <= minDistance;

        // Calculate rotation and translation
        deltaRotationX += rotationControl.x * rotateSencetive;
        deltaRotationY += rotationControl.y * -rotateSencetive;

        deltaRotationY = ClampAngle(deltaRotationY, minVerticleAngle, maxVerticleAngle);

      

        Quaternion finalRotation = Quaternion.Euler(deltaRotationY, deltaRotationX, 0);
        Vector3 finalPosition = vehicle.transform.position - (finalRotation * Vector3.forward * distance);
        finalPosition = AddLocalOffset(finalPosition);


        // Calculate current distance
        float targetDistance = distance;

        RaycastHit hit;

        Debug.DrawLine(vehicle.transform.position + new Vector3(0, Offset.y, 0), finalPosition, Color.red);

        if (Physics.Linecast(vehicle.transform.position + new Vector3(0,Offset.y, 0), finalPosition, out hit) == true)
        {
            float distanceToHit = Vector3.Distance(vehicle.transform.position + new Vector3(0, Offset.y, 0), hit.point);
            if (hit.transform != vehicle)
            {
                if (distanceToHit < distance)
                {
                    targetDistance = distanceToHit - distanceOffsetFromCollisionHit;
                }
            }
         
        }

        currentDistance = Mathf.MoveTowards(currentDistance, targetDistance, Time.deltaTime * distanceLerpRate);

        currentDistance = Mathf.Clamp(currentDistance, minDistance, distance);

        // Correct camera position 
        finalPosition = vehicle.transform.position - (finalRotation * Vector3.forward * currentDistance);



        // Apply transform
        transform.rotation = finalRotation;
        transform.position = finalPosition;
        transform.position = AddLocalOffset(transform.position);

        // Zoom

        zoomMaskEffect.SetActive(isZoom);
        if (isZoom == true)
        {
            transform.position = vehicle.ZoomOpticsPosition.position;
            camera.fieldOfView = zoomedFov;
            maxVerticleAngle = zoomedMaxVerticalAngle;

        }
        else
        {
            camera.fieldOfView = defaultFov;
            maxVerticleAngle = defaultMaxVerticaLAngle;
        }


    }

    private void UpdateControle()
    {
        rotationControl = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        distance += -Input.mouseScrollDelta.y * scrollSencetive;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isZoom = !isZoom;

            if (isZoom == true)
            {
                lastDistance = distance;
                distance = minDistance;
            }
            else
            {
                distance = lastDistance;
                currentDistance = distance;
            }
        }
    }

    private Vector3 AddLocalOffset(Vector3 position)
    {
        Vector3 result = position;
        result += new Vector3(0, Offset.y, 0);
        result += transform.right * Offset.x;
        result += transform.forward * Offset.z;

        return result;
    }



    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        
        return Mathf.Clamp(angle, min, max); 
    }


    public void SetTarget(Vehicle target)
    {
        this.vehicle = target;
    }

}
