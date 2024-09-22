using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bean : MonoBehaviour
{
    private Transform m_camTransform;

    private void Awake()
    {
        m_camTransform = Camera.main.transform;
    }

    private void Update()
    {
        // rotate to face the camera but don't rotate up and down
        Vector3 targetPosition = m_camTransform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition, Vector3.up);
    }
}
