using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NsSoftRenderer;

public class SoftCameraTest : MonoBehaviour
{
    public Mesh sharedMesh = null;

    private Camera m_UnityCam = null;
    private SoftCamera m_SoftCam = null;
    private List<Vector3> m_VecList = null;
    private int[] m_TriangleIndexes = null;
    private int[] m_Indexes = null;
    // Start is called before the first frame update
    void Start()
    {
        m_UnityCam = Camera.main;
        m_SoftCam = SoftCamera.MainCamera;
    }

    public static string GetVectorStr(Vector3 vec) {
        string ret = string.Format("x: {0}  y: {1}  z: {2}", vec.x.ToString(), vec.x.ToString(), vec.z.ToString());
        return ret;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_UnityCam != null && m_SoftCam != null) {
            // 比较
            var trans = this.transform;
            var pt = trans.position;

            /*
            var uPt = m_UnityCam.WorldToViewportPoint(pt);
            var mat = m_SoftCam.ViewProjMatrix;
            var sPt = mat.MultiplyPoint(pt);
            Debug.LogFormat("【Proj】【Unity】{0}【SoftCamera】{0}", GetVectorStr(uPt), GetVectorStr(sPt));

            uPt = m_UnityCam.WorldToScreenPoint(pt);
            mat = m_SoftCam.ViewProjLinkerScreenMatrix;
            sPt = mat.MultiplyPoint(pt);
            Debug.LogFormat("【Screen】【Unity】{0}【SoftCamera】{0}", GetVectorStr(uPt), GetVectorStr(sPt));
           */ 
            
        if (sharedMesh != null ) {
                if (m_VecList == null) {
                    m_VecList = new List<Vector3>();
                    sharedMesh.GetVertices(m_VecList);
                }

                if (m_TriangleIndexes == null && sharedMesh.subMeshCount > 0)
                    m_TriangleIndexes = sharedMesh.GetTriangles(0);
               
                if (m_Indexes == null && sharedMesh.subMeshCount > 0) {
                    m_Indexes = sharedMesh.GetIndices(0);
                }

                if (m_Indexes != null && m_Indexes.Length > 0 && m_VecList != null && m_VecList.Count > 0 && m_TriangleIndexes != null && m_TriangleIndexes.Length > 0) {
                    for (int i = 0; i < m_TriangleIndexes.Length / 3; ++i) {
                        Triangle tri1 = new Triangle();
                        tri1.p1 = m_VecList[m_TriangleIndexes[i * 3]];
                        tri1.p2 = m_VecList[m_TriangleIndexes[i * 3 + 1]];
                        tri1.p2 = m_VecList[m_TriangleIndexes[i * 3 + 2]];

                        tri1.MulMatrix(this.transform.localToWorldMatrix);

                        Triangle tri2 = tri1;

                        // tri1.MulMatrix(m_UnityCam.projectionMatrix * m_UnityCam.worldToCameraMatrix);
                        tri1.p1 = m_UnityCam.WorldToScreenPoint(tri1.p1);
                        tri1.p2 = m_UnityCam.WorldToScreenPoint(tri1.p2);
                        tri1.p3 = m_UnityCam.WorldToScreenPoint(tri1.p3);

                        tri2.MulMatrix(m_SoftCam.ViewProjLinkerScreenMatrix);
                        Debug.LogFormat("【Unity】{0}【SoftCam】{1}", tri1.ToString(), tri2.ToString());
                    }
                }
            }

            /*
            bool isContains = SoftMath.PtInCamera(pt, m_SoftCam);
            Debug.LogFormat("【SoftCamera】{0}", isContains.ToString());
            */
        }
    }
}
