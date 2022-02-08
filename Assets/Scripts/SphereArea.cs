using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereArea : MonoBehaviour
{
    [SerializeField] private float radius;

    [SerializeField] private Color color = Color.green;

    public Vector3 RandomInsise
    {
        get
        {
            var pos = UnityEngine.Random.insideUnitSphere * radius + transform.position;

            pos.y = transform.position.y;

            return pos;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = color;   

        Gizmos.DrawSphere(transform.position, radius);
    }
}
