using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMapTest : MonoBehaviour
{

    float _CubeSize = 1.0f;

    bool CheckPt(Vector3 pt) {
        float halfSize = _CubeSize / 2.0f;
        return (pt.z >= 0) && (pt.z <= _CubeSize) && (pt.x >= -halfSize) && (pt.x <= halfSize) && (pt.y >= -halfSize) && (pt.y <= halfSize);
    }

    bool CheckPlanePt(Vector3 dir, Vector3 org, Vector3 pnlNormal, Vector3 pnlPt, out Vector3 pt) {
        pt = Vector3.zero;

        Vector3 p = pnlPt - org;
        float div = (dir.x * pnlNormal.x + dir.y * pnlNormal.y + dir.z * pnlNormal.z);
        if (div == 0)
            return false;
        float t = (p.x * pnlNormal.x + p.y * pnlNormal.y + p.z * pnlNormal.z) / div;
        if (t <= 0)
            return false;
        pt = org + dir * t;
        return CheckPt(pt);
        //return true;
    }

    private void Start() {
        // TestMeshVertexs();

        TestCam(Vector3.zero);

    }

    void TestCam(Vector3 model_vertex) {
        var mainCam = Camera.main;
        if (mainCam == null)
            return;
        //   Vector3 camPos = mainCam.transform.position;
        //   camPos = this.transform.worldToLocalMatrix.MultiplyPoint(camPos);

        Vector3 camPos = new Vector3(0, 0, -1f);
        TestPt(camPos, model_vertex);
    }

    void TestPt(Vector3 model_CamPos, Vector3 model_vertex) {
        Vector3 org = model_CamPos;

        Vector3 dir = model_vertex - org;

        float halfSize = _CubeSize / 2.0f;
        Vector3 reflectCenter = new Vector3(0, 0, halfSize);

        Vector3 pnlNormal = new Vector3(1.0f, 0, 0);
        Vector3 pnlPt = new Vector3(-halfSize, 0, 0);
        Vector3 pt;
        bool isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, out pt);
        if (isInPnl) {
            Vector3 reflectDir = (pt - reflectCenter).normalized;
            Debug.LogError(reflectDir.ToString());
           // col = texCUBE(_MainTex, reflectDir);
        } else {
            // back panel

            pnlNormal = new Vector3(0, 0, -1f);
            pnlPt = new Vector3(-halfSize, 0, _CubeSize);
            isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, out pt);
            if (isInPnl) {
                Vector3 reflectDir = (pt - reflectCenter).normalized;
                //  col = texCUBE(_MainTex, reflectDir);
                Debug.LogError(reflectDir.ToString());
            } else {


                // right panel
                pnlNormal = new Vector3(-1, 0, 0);
                pnlPt = new Vector3(halfSize, 0, 0);
                isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, out pt);
                if (isInPnl) {
                    Vector3 reflectDir = (pt - reflectCenter).normalized;
                    // col = texCUBE(_MainTex, reflectDir);
                    Debug.LogError(reflectDir.ToString());
                } else {
                    // top panel
                    pnlNormal = new Vector3(0, -1, 0);
                    pnlPt = new Vector3(0, halfSize, 0);
                    isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, out pt);
                    if (isInPnl) {
                        Vector3 reflectDir = (pt - reflectCenter).normalized;
                        //  col = texCUBE(_MainTex, reflectDir);
                        Debug.LogError(reflectDir.ToString());
                    }
                }


            }

        }
    }

    void TestMeshVertexs() {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null) {
            var mesh = filter.sharedMesh;
            if (mesh != null) {
                List<Vector3> vertexs = new List<Vector3>();
                mesh.GetVertices(vertexs);
                for (int i = 0; i < vertexs.Count; ++i) {
                    Debug.LogError(vertexs[i]);
                }
            }
        }
    }
}
