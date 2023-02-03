using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private float _maximumForce;

    [SerializeField]
    private float _maximumForceTime;

    private float _timeMouseButtonDown;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _timeMouseButtonDown = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray,out RaycastHit hitInfo))
            {
                RagdollGuy guy = hitInfo.collider.GetComponentInParent<RagdollGuy>();

                if(guy != null)
                {
                    float mouseButtonDownDuration = Time.time - _timeMouseButtonDown;
                    float forcePercentage = mouseButtonDownDuration / _maximumForceTime;
                    float forceMagnitude = Mathf.Lerp(1, _maximumForce, forcePercentage);
                    print("<color=#00FFEA>ForceUsed:</color>" + forceMagnitude);

                    Vector3 forceDir = guy.transform.position - _camera.transform.position;
                    forceDir.y = 1;
                    forceDir.Normalize();

                    Vector3 force = forceMagnitude * forceDir;

                    guy.TriggerRagdoll(force, hitInfo.point);
                }

            }
        }
    }
}
