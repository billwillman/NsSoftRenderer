using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NsSoftRenderer;
using System.IO;

public class SoftCameraTest : MonoBehaviour
{
    public bool IsShowSoftCamerLog = false;

    public float EditorX = 0f;
    public float EditorY = 0f;
    private float m_LastEditorY = 0f;
    private bool m_IsEditorInited = false;

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

            if (m_UnityCam != null && m_SoftCam != null) {
                /*
                Triangle tri1 = new Triangle(
                    new Vector3(-0.6f, 0f, -8.6f),
                    new Vector3(0.8f, 1.0f, -10.1f),
                    new Vector3(0.8f, 0f, -10.1f)
                    );
                Triangle tri2 = tri1;
                tri1.Trans(m_UnityCam.WorldToScreenPoint);
                // tri2.MulMatrix(m_SoftCam.ViewProjMatrix);
                tri2.Trans(m_SoftCam.WorldToScreenPointEvt);

                Debug.LogErrorFormat("【Unity】{0}【Soft】{1}", tri1.ToString(), tri2.ToString());
                */

                Triangle tri = new Triangle(
                    new Vector3(-100f, 0, 0),
                    new Vector3(100f, 0, 0),
                    new Vector3(100f, 100f, 0)
                    );

                 var bottomTop = tri.p3 - tri.p1;
                var middleTop = tri.p3 - tri.p2;

                  EditorY = Mathf.Clamp(EditorY, tri.p1.y, tri.p3.y);
                bool isResetX = (Mathf.Abs(m_LastEditorY - EditorY) > float.Epsilon) || !m_IsEditorInited;
                m_LastEditorY = EditorY;

                  float startX = RenderTarget.GetVector2XFromY(bottomTop, tri.p1, EditorY);
                float endX = RenderTarget.GetVector2XFromY(middleTop, tri.p2, EditorY);
                if (isResetX)
                    EditorX = startX;
                EditorX = Mathf.Clamp(EditorX, startX, endX);
                m_IsEditorInited = true;

                Vector3 P = new Vector3(EditorX, EditorY, 0f);
                float a, b, c;
                SoftMath.GetBarycentricCoordinate(tri, P, out a, out b, out c);
                Vector3 PP = tri.p1 * a +tri.p2 * b + tri.p3 * c;

                Debug.LogErrorFormat("a: {0}, b: {1}, c: {2} || {3}", a.ToString(), b.ToString(), c.ToString(), PP.ToString2());
            }


        }
    }
}
