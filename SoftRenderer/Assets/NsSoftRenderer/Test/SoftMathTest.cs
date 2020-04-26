using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NsSoftRenderer;

public class SoftMathTest : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 A = new Vector3(-1, 0, 0);
        Vector3 B = new Vector3(0, 1, 0);
        Vector3 C = new Vector3(0, 0, 1);
        float u = Random.Range(0f, 1f);
        float v = Random.Range(0f, 1f - u);
        float r = 1f - u - v;
        Vector3 P = u * A + v * B + r * C;

        float u1, v1, r1;
        SoftMath.GetBarycentricCoordinate(ref A, ref B, ref C, ref P, out u1, out v1, out r1);
        Debug.LogFormat("【origin】u: {0} v: {1} r: {2}【SoftMth】u: {3} v: {4} r: {5}", u, v, r, u1, v1, r1);

        Vector3 AB = B - A;
        Vector3 AC = C - A;
        Vector3 PA = A - P;
        Vector3 v3 = new Vector3(AB.z, AC.z, PA.z);
        Debug.LogFormat("v3: {0}", v3.x * u1, v3.y * v1, v3.z * r1);
    }
}
