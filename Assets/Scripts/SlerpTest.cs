using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlerpTest : MonoBehaviour
{
    public Transform StartTr, EndTr, MovingLerp, MovingObjSle;
    public int LerpQuality = 10;
    public float offset = 1;
    public Vector3 Direction = new Vector3(0, 1, 0);
    private Vector3 center = Vector3.zero, relStart, relEnd;
    private void Update()
    {
        if (StartTr == null || EndTr == null) return;
        center = (StartTr.position + EndTr.position) * 0.5f;
        center -= (Direction * offset);
        relStart = StartTr.position - center;
        relEnd = EndTr.position - center;
        float t = (Mathf.Sin(Time.time) + 1) / 2;

        Vector3 lerpPos = Vector3.Lerp(StartTr.position, EndTr.position, t);
        Vector3 slerpPos = Vector3.Slerp(relStart, relEnd, t) + center;

        MovingLerp.position = lerpPos;
        MovingObjSle.position = slerpPos;
    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(center, Vector3.one * 0.25f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + Direction);
    }
}
