using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ListTest : MonoBehaviour
{
    private NativeList<Vector3> testList = new NativeList<Vector3>();

    private void Start() {
        testList.Add(new Vector3(1, 1, 1));
    }

    private void OnDestroy() {
        testList.Dispose();
    }
}
