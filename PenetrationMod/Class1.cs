using System.Collections;
using MelonLoader;
using UnityEngine;
using Il2CppProperties;
using Il2CppDynamicGUI;

[assembly: MelonInfo(typeof(PenetrationMod.PenetrationModMain), "Penetration Limit Modifier", "1.1.0", "furryAxw")]
[assembly: MelonGame("HD", "Sprocket")]

namespace PenetrationMod
{
    public class PenetrationModMain : MelonMod
    {
        // 用于记录当前是否在设计器模式中
        private bool isInDesigner = false;
        // 用于保存协程引用，方便在退出场景时立刻停止它
        private object? monitorCoroutine;

        private float NewMaxPenetration = 2000f;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "VehicleDesignerUI")
            {
                isInDesigner = true;

                // 启动持续监测协程
                monitorCoroutine = MelonCoroutines.Start(MonitorSlidersContinuous());
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (sceneName == "VehicleDesignerUI")
            {
                isInDesigner = false;

                // 退出场景时停止协程，释放性能
                if (monitorCoroutine != null)
                {
                    MelonCoroutines.Stop(monitorCoroutine);
                    monitorCoroutine = null;
                }
            }
        }

        private IEnumerator MonitorSlidersContinuous()
        {
            // 只要还在编辑模式，就一直循环检测
            while (isInDesigner)
            {
                // 寻找 ConfigPanel (仅查找当前激活的对象，性能开销小)
                GameObject configPanel = GameObject.Find("ConfigPanel");

                if (configPanel != null)
                {
                    // 获取 ConfigPanel 下所有的 PropertyFieldSlider 组件 (包含被隐藏的)
                    var sliders = configPanel.GetComponentsInChildren<PropertyFieldSlider>(true);

                    foreach (var slider in sliders)
                    {
                        FloatProperty prop = slider.prop;

                        if (prop != null && (prop.Name == "Penetration" || prop.name == "Penetration"))
                        {
                            // 检查当前值，如果已经被修改过了，就不再重复赋值和打印日志
                            if (prop.Max != NewMaxPenetration)
                            {
                                try
                                {
                                    prop.Max = NewMaxPenetration;
                                }
                                catch (System.Exception ex)
                                {
                                    MelonLogger.Error($"修改属性时发生错误: {ex.Message}");
                                }
                            }

                            // 既然已经找到了 Penetration 并处理完毕，可以跳出当前 foreach 循环
                            break;
                        }
                    }
                }

                // 每隔 0.5 秒检测一次。这个频率对 UI 响应来说足够快，且对游戏帧数几乎零影响。
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}