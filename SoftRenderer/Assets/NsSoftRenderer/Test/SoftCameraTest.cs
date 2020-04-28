using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NsSoftRenderer;

public class SoftCameraTest : MonoBehaviour
{

    private Camera m_UnityCam = null;
    private SoftCamera m_SoftCam = null;
    // Start is called before the first frame update
    void Start()
    {
        m_UnityCam = Camera.main;
        m_SoftCam = SoftCamera.MainCamera;
    }

    private string GetVectorStr(Vector3 vec) {
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

            
            var uPt = m_UnityCam.WorldToViewportPoint(pt);
            var mat = m_SoftCam.ViewProjMatrix;
            var sPt = mat * pt;
            Debug.LogFormat("【Proj】【Unity】{0}【SoftCamera】{0}", GetVectorStr(uPt), GetVectorStr(sPt));

            uPt = m_UnityCam.WorldToScreenPoint(pt);
            mat = m_SoftCam.ViewProjLinkerScreenMatrix;
            sPt = mat * pt;
            Debug.LogFormat("【Screen】【Unity】{0}【SoftCamera】{0}", GetVectorStr(uPt), GetVectorStr(sPt));
            

            /*
            bool isContains = SoftMath.PtInCamera(ref pt, m_SoftCam);
            Debug.LogFormat("【SoftCamera】{0}", isContains.ToString());
            */
        }
    }
}
