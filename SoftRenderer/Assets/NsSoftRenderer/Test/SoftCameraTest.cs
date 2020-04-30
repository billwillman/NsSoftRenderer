using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NsSoftRenderer;
using System.IO;

public class SoftCameraTest : MonoBehaviour
{
    public Mesh sharedMesh = null;
    public bool IsShowSoftCamerLog = false;

    private Camera m_UnityCam = null;
    private SoftCamera m_SoftCam = null;
    private List<Vector3> m_VecList = null;
    private int[] m_TriangleIndexes = null;
    private int[] m_Indexes = null;
  //  private FileStream stream = null;
    // Start is called before the first frame update

    public static string GetVectorStr(Vector3 vec) {
        string ret = string.Format("x: {0}  y: {1}  z: {2}", vec.x.ToString(), vec.x.ToString(), vec.z.ToString());
        return ret;
    }

    private void OnDestroy() {
    }

    void CheckSoftCameraLog() {
        if (m_SoftCam != null)
            m_SoftCam.IsShowVertexLog = IsShowSoftCamerLog;
    }

    // Update is called once per frame
    void Update()
    {
        m_UnityCam = Camera.main;
        m_SoftCam = SoftCamera.MainCamera;

        CheckSoftCameraLog();

        if (!IsShowSoftCamerLog) {
            m_UnityCam = Camera.main;

            if (m_UnityCam != null && m_SoftCam != null && sharedMesh != null) {

                if (m_VecList == null) {
                    m_VecList = new List<Vector3>();
                    sharedMesh.GetVertices(m_VecList);
                }

                if (m_TriangleIndexes == null && sharedMesh.subMeshCount > 0)
                    m_TriangleIndexes = sharedMesh.GetTriangles(0);

                if (m_Indexes == null && sharedMesh.subMeshCount > 0) {
                    m_Indexes = sharedMesh.GetIndices(0);
                }

                // 比较
                var trans = this.transform;
                var pt = trans.position;

                if (m_VecList != null && m_VecList.Count > 0 && m_TriangleIndexes != null && m_TriangleIndexes.Length > 0) {

                   // Debug.LogErrorFormat("[Unity]{0}  [SoftCamera]{1}", m_UnityCam.projectionMatrix, m_SoftCam.ProjMatrix);

                    for (int i = 0; i < (int)m_TriangleIndexes.Length / 3; ++i) {
                        Triangle tt1 = new Triangle();
                        tt1.p1 = m_VecList[m_TriangleIndexes[i * 3]];
                        tt1.p2 = m_VecList[m_TriangleIndexes[i * 3 + 1]];
                        tt1.p3 = m_VecList[m_TriangleIndexes[i * 3 + 2]];

                        tt1.MulMatrix(trans.localToWorldMatrix);

                        Triangle tt2 = tt1;

                        
                        //tt1.MulMatrix(m_UnityCam.projectionMatrix * m_UnityCam.worldToCameraMatrix);
                        tt1.Trans(m_UnityCam.WorldToScreenPoint);

                        tt2.MulMatrix(m_SoftCam.ViewProjLinkerScreenMatrix);

                      //  Debug.Log("[Test]" + m_SoftCam.ViewProjLinkerScreenMatrix.ToString());

                        Debug.LogErrorFormat("【Proj】【Unity】{0}【SoftCamera】{1}", tt1.ToString(), tt2.ToString());
                    }
                }
            }
        }
    }
}
