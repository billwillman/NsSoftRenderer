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
            Debug.LogFormat("【Unity】{0}【SoftCamera】{0}", uPt.ToString(), sPt.ToString());
        }
    }
}
