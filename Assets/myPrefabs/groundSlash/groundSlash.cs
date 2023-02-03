using System.Collections;
using UnityEngine;

public class groundSlash : MonoBehaviour
{
    public float speed = 10f;
    public float slowDownPercent = 0.01f;
    public float detectDistance = 0.1f;
    public float destroyDelay = 5;

    private Rigidbody rb;
    private bool stopped = false;
    WaitForSeconds wfs;

    void Start()
    {
        rb = GetComponent<Rigidbody>() ?? null; //checa si hay, si si lo iguala, sino lo deja null.
        StartCoroutine(slowDown());

        Destroy(gameObject, destroyDelay);
    }

    private void FixedUpdate()
    {
        if (!stopped)
        {
            RaycastHit hit;
            Vector3 originPoint = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            if(Physics.Raycast(originPoint,transform.TransformDirection(-Vector3.up),out hit, detectDistance))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
            Debug.DrawRay(originPoint, transform.TransformDirection(-Vector3.up * detectDistance), Color.red);
        }
    }

    IEnumerator slowDown()
    {     
        if (slowDownPercent > 0 && rb!=null)
        {
            wfs = new WaitForSeconds(0.05f);
            float t = 1;
            while (t > 0)
            {
                rb.velocity = Vector3.Lerp(Vector3.zero, rb.velocity, t);
                t -= slowDownPercent;
                yield return wfs;
            }
            stopped = true;
        }

        stopped = true;
    }
}
