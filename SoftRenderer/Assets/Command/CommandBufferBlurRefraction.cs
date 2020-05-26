using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEngine;
#endif
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CommandBufferBlurRefraction : MonoBehaviour
{
    public Shader blurShader;
    private Material material;

    private Camera cam;
    //存储Camera和对应的Command Buffer的键值对
    private Dictionary<Camera, CommandBuffer> cameras = new Dictionary<Camera, CommandBuffer>();

    private int blurredID;
    private int blurredID2;
    private CommandBuffer buf;

    //移除之前在所有摄像机添加的Command Buffer
    //private void Cleanup()
    //{
    //    foreach (var cam in cameras)
    //    {
    //        if (cam.Key)
    //        {
    //            //移除在指定的CameraEvent添加的指定的Command Buffer
    //            cam.Key.RemoveCommandBuffer(CameraEvent.AfterSkybox, cam.Value);
    //        }
    //    }
    //    //清空字典
    //    cameras.Clear();
    //    //删除材质球
    //    Object.DestroyImmediate(material);
    //}

    //启用组件时清空一次
    public void OnEnable()
    {
        //Cleanup();
        //如果material为空,则创建一个材质球使用 BlurShader
        if (!material)
        {
            material = new Material(blurShader);
            //设置材质球的标签,隐藏并且不保存 HideAndDontSave;
            material.hideFlags = HideFlags.HideAndDontSave;
        }
        //如果当前脚本物体没有激活,或者脚本没有激活,则直接返回不在运行
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            //Cleanup();
            return;
        }

    }

    private void OnDestroy() {
        var iter = cameras.GetEnumerator();
        while (iter.MoveNext()) {
            var cam = iter.Current.Key;
            if (cam != null) {
                cam.RemoveCommandBuffer(CameraEvent.AfterSkybox, iter.Current.Value);
            }
        }
        iter.Dispose();

        cameras.Clear();
    }

    //禁用组件时清空一次
    //public void OnDestroy()
    //{
    //    //Cleanup();
    //}

    void OnBecameInvisible() {
        cam = Camera.current;
        if (!cam) {
            return;
        }
        CommandBuffer buffer;
        if (cameras.TryGetValue(cam, out buffer)) {
            cam.RemoveCommandBuffer(CameraEvent.AfterSkybox, buffer);
            cameras.Remove(cam);
        }
    }

    //当任何摄像机要渲染该对象的时候,为这个相机添加一个Command Buffer
    // public void OnWillRenderObject()

   void OnBecameVisible() {
      
        //如果当前没有摄像机渲染,则直接返回
        cam = Camera.current;
        if (!cam)
        {
            return;
        }

        buf = null;

        //如果已经将当前摄像机添加到字典中,说明已经添加过Command Buffer 直接返回
        //if (cameras.ContainsKey(cam))
        //{
        //    return;
        //}

        //创建一个新的Command Buffer
        //与当前摄像机的cam组成键值对,添加到字典中
        buf = new CommandBuffer();
        buf.name = "Grab screen and blur";
        //这种写法等价于 Cameras.Add(cam,buf);
        cameras[cam] = buf;

        //将当前屏幕复制到一个临时Render Texture
        //设置RT的ID名称,根据shader的变量名获取
        //使用属性ID在调用材质球的属性时更高效
        //在不同硬件上属性ID不同,所以不要试图存储或者通过网络发送
        int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");

        //添加一个临时RenderTexture
        //GetTemporaryRT 创建一个临时的RT，根据属性ID设置为全局shader参数
        //临时RT如果未调用,ReleaseTemporaryRT显式释放
        //在摄像机渲染完成或Graphics.ExcuteCommandBuffer执行完成之后被移除
        //width和height参数是贴图的宽与高,如果设置为-1则使用摄像机的宽高
        //-x则为摄像机的宽高 / x
        buf.GetTemporaryRT(screenCopyID, -2, -2, 0, FilterMode.Bilinear);

        //复制当前激活的RT到上一步创建的临时RT
        buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

        //获取两个更小的RT
        blurredID = Shader.PropertyToID("_Temp1");
        blurredID2 = Shader.PropertyToID("_Temp2");
        //-3 等于摄像机宽高 / 3
        buf.GetTemporaryRT(blurredID, -3, -3, 0, FilterMode.Bilinear);
        buf.GetTemporaryRT(blurredID2, -3, -3, 0, FilterMode.Bilinear);

        //将临时RT复制到上一步中创建的两个更小的RT中,同时释放临时RT
        buf.Blit(screenCopyID, blurredID);
        buf.ReleaseTemporaryRT(screenCopyID);
        //高斯模糊
        //水平模糊 两个像素
        buf.SetGlobalVector("offsets", new Vector4(2.0f / Screen.width, 0, 0, 0));
        buf.Blit(blurredID, blurredID2, material);
        //垂直二分模糊 两个像素
        buf.SetGlobalVector("offsets", new Vector4(0,2.0f / Screen.height, 0, 0));
        buf.Blit(blurredID2, blurredID, material);
        ////水平模糊 四个像素
        //buf.SetGlobalVector("offsets", new Vector4(4.0f / Screen.width, 0, 0, 0));
        //buf.Blit(blurredID, blurredID2, material);
        ////垂直二分模糊 四个像素
        //buf.SetGlobalVector("offsets", new Vector4(0,4.0f / Screen.height, 0, 0));
        //buf.Blit(blurredID2, blurredID, material);
        //输出模糊结果
        buf.SetGlobalTexture("_GrabBlurTexture", blurredID);

        //在渲染完不透明物体和天空合之后执行Command Buffer
        cam.AddCommandBuffer(CameraEvent.AfterSkybox, buf);

    }
}
