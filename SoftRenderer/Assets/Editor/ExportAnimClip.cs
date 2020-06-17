using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExportAnimClip : Editor
{
    public static int totalNum = 0;

    public static void ExportAnimClips(string abFileName, bool isTotalFiles, ref int num, bool isLegacy = false, bool copySerier = true) {
        if (isTotalFiles)
            num = 0;
        
        if (string.IsNullOrEmpty(abFileName))
            return;
        abFileName = abFileName.Replace('\\', '/');
        if (abFileName.IndexOf("/StreamingAssets/", StringComparison.CurrentCultureIgnoreCase) < 0) {
            Debug.LogError("not StreamingAssets~!");
            return;
        }
        var ab = AssetBundle.LoadFromFile(abFileName);
        if (ab == null) {
            Debug.LogError("AB not found~!");
            return;
        }
        try {
            string parentDir = Path.GetDirectoryName(abFileName).Replace('\\', '/');
            string outDir = parentDir + "/AnimClips";

            string title = string.Format("导出动画文件 AB: {0}", Path.GetFileNameWithoutExtension(abFileName));
            // float process = (float)i / (float)files.Length;
            // EditorUtility.DisplayProgressBar("导出动画", title, process);

            bool isCheck = true;
            var clips = ab.LoadAllAssets<AnimationClip>();
            if ((clips != null) && (clips.Length > 0)) {
                if (isTotalFiles) {
                    num += clips.Length;
                    return;
                }
                bool isChanged = false;
                for (int i = 0; i < clips.Length; ++i) {
                    var clip = clips[i];
                    if (clip != null) {
                        // Debug.LogFormat("导出=》{0}", clip.name);
                        num += 1;
                        float process = (float)num / (float)totalNum;
                        string info = clip.name + ".anim";
                        EditorUtility.DisplayProgressBar(title, info, process);

                        var newClip = new AnimationClip();
                        newClip.name = clip.name;//设置新clip的名字
                        var setting = AnimationUtility.GetAnimationClipSettings(clip);//获取AnimationClipSettings
                        AnimationUtility.SetAnimationClipSettings(newClip, setting);//设置新clip的AnimationClipSettings
                        newClip.frameRate = clip.frameRate;//设置新clip的帧率

                        if (!copySerier) {
                            EditorCurveBinding[] curveBindings;
                            if (isLegacy)
                                curveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                            else
                                curveBindings = AnimationUtility.GetCurveBindings(clip);//获取clip的curveBinds
                            for (int j = 0; j < curveBindings.Length; ++j) {
                                if (isLegacy) {
                                    var binding = curveBindings[j];
                                    var curve = AnimationUtility.GetObjectReferenceCurve(clip, curveBindings[j]);
                                    AnimationUtility.SetObjectReferenceCurve(newClip, binding, curve);//设置新clip的curve
                                } else {
                                    var binding = curveBindings[j];
                                    var curve = AnimationUtility.GetEditorCurve(clip, curveBindings[j]);
                                    AnimationUtility.SetEditorCurve(newClip, binding, curve);//设置新clip的curve
                                }
                            }
                        } else {
                            EditorUtility.CopySerialized(clip, newClip);
                        }

                        string clipFileName = string.Format("{0}/{1}.anim", outDir, clip.name);
                        if (isCheck) {
                            isCheck = false;
                            if (!Directory.Exists(outDir)) {
                                AssetDatabase.CreateFolder(parentDir, "AnimClips");
                            }
                        }
                        if (File.Exists(clipFileName)) {
                            File.Delete(clipFileName);
                        }
                        AssetDatabase.CreateAsset(newClip, clipFileName);
                        AssetDatabase.SaveAssets();
                        isChanged = true;
                    }
                }
                if (isChanged)
                    AssetDatabase.Refresh();
            }
        } finally {
            ab.Unload(true);
        }
    }

    [MenuItem("Assets/测试BindPose")]
    public static void TestBindPose() {
        var gameObj = Selection.activeGameObject;
        if (gameObj != null) {
            var skl = gameObj.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skl != null) {
                Debug.Log(skl.sharedMesh.bindposes.ToString());
            }
        }
    }

    static string startStr = "Assets/StreamingAssets/npc/npc_baboben_1";
    static bool isLegacy = false;
    static bool copySerier = false;

    [MenuItem("Assets/导出测试的AB的AnimClips")]
    public static void TestExportAnimClips() {
       // string abFileName = AssetDatabase.GetAssetPath(Selection.activeObject);
        string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*.dat", SearchOption.AllDirectories);


        

        // 先计算文件内容
        totalNum = 0;
        for (int i = 0; i < files.Length; ++i) {
            string abFileName = files[i].Replace('\\', '/');
            int idx = abFileName.IndexOf(startStr, StringComparison.CurrentCultureIgnoreCase);
            if (idx >= 0) {
                abFileName = abFileName.Substring(idx);
                int num = 0;
                ExportAnimClips(abFileName, true, ref num, isLegacy, copySerier);
                totalNum += num;
            }
        }

       //  string abFileName = Application.streamingAssetsPath + "/pack_1.dat";
       
        try {

            int num = 0;
            for (int i = 0; i < files.Length; ++i) {
                //string title = string.Format("AB: {0}", Path.GetFileNameWithoutExtension(files[i]));
               // float process = (float)i / (float)files.Length;
               // EditorUtility.DisplayProgressBar("导出动画", title, process);
                string abFileName = files[i].Replace('\\', '/');
                int idx = abFileName.IndexOf(startStr, StringComparison.CurrentCultureIgnoreCase);
                if (idx >= 0) {
                    abFileName = abFileName.Substring(idx);      
                    ExportAnimClips(abFileName, false, ref num, isLegacy, copySerier);
                }
            }
        } finally {
            EditorUtility.ClearProgressBar();
        }
    }
}
