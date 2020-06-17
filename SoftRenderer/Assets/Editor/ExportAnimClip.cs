using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExportAnimClip : Editor
{
    public static void ExportAnimClips(string abFileName) {
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
            
            bool isCheck = true;
            var clips = ab.LoadAllAssets<AnimationClip>();
            if ((clips != null) && (clips.Length > 0)) {
                bool isChanged = false;
                for (int i = 0; i < clips.Length; ++i) {
                    var clip = clips[i];
                    if (clip != null) {
                        Debug.LogFormat("导出=》{0}", clip.name);

                        var newClip = new AnimationClip();
                        newClip.name = clip.name;//设置新clip的名字
                        var setting = AnimationUtility.GetAnimationClipSettings(clip);//获取AnimationClipSettings
                        AnimationUtility.SetAnimationClipSettings(newClip, setting);//设置新clip的AnimationClipSettings
                        newClip.frameRate = clip.frameRate;//设置新clip的帧率
                        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);//获取clip的curveBinds
                        for (int j = 0; j < curveBindings.Length; ++j) {
                            AnimationUtility.SetEditorCurve(newClip, curveBindings[j], AnimationUtility.GetEditorCurve(clip, curveBindings[j]));//设置新clip的curve
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

    [MenuItem("Assets/导出测试的AB的AnimClips")]
    public static void TestExportAnimClips() {
       // string abFileName = AssetDatabase.GetAssetPath(Selection.activeObject);
        string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*.dat", SearchOption.AllDirectories);


        //  string abFileName = Application.streamingAssetsPath + "/pack_1.dat";
        string startStr = "Assets/StreamingAssets/";
        try {
            for (int i = 0; i < files.Length; ++i) {
                string title = string.Format("AB: {0}", Path.GetFileNameWithoutExtension(files[i]));
                float process = (float)i / (float)files.Length;
                EditorUtility.DisplayProgressBar("导出动画", title, process);
                string abFileName = files[i].Replace('\\', '/');
                int idx = abFileName.IndexOf(startStr, StringComparison.CurrentCultureIgnoreCase);
                if (idx >= 0) {
                    abFileName = abFileName.Substring(idx);
                    ExportAnimClips(abFileName);
                }
            }
        } finally {
            EditorUtility.ClearProgressBar();
        }
    }
}
