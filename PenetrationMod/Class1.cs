using System.Collections;
using MelonLoader;
using UnityEngine;
using Il2CppProperties;
using Il2CppDynamicGUI;

[assembly: MelonInfo(typeof(PenetrationMod.PenetrationModMain), "Penetration Limit Modifier", "1.2.0", "furryAxw")]
[assembly: MelonGame("HD", "Sprocket")]

namespace PenetrationMod
{
    public class PenetrationModMain : MelonMod
    {
        // 用于记录当前是否在设计器模式中
        private bool isInDesigner = false;
        // 用于保存协程引用，方便在退出场景时立刻停止它
        private object? monitorCoroutine;

        // 逻辑极限值
        private float NewMaxPenetration = 20000f;
        private float NewMinCaliber = 1f;
        private float NewMaxCaliber = 5000f;

        // UI 拖动条极限值
        private float SliderNewMaxPenetration = 1000f;
        private float SliderNewMinCaliber = 1f;
        private float SliderNewMaxCaliber = 500f;

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
            // 性能优化：缓存 GameObject，避免高频调用 GameObject.Find
            GameObject configPanel = null;

            while (isInDesigner)
            {
                if (configPanel == null)
                {
                    configPanel = GameObject.Find("ConfigPanel");
                    // 如果当前帧没找到（UI可能还没实例化完），等待下一帧再试
                    if (configPanel == null)
                    {
                        yield return new WaitForSeconds(0.5f);
                        continue;
                    }
                }

                // 获取所有的 PropertyFieldSlider
                var propertySliders = configPanel.GetComponentsInChildren<PropertyFieldSlider>(true);

                // 根据你提供的信息，如果不想依赖 prop.Name，也可以通过索引判断
                // 但通过 prop.Name 判定是最具备鲁棒性的做法
                foreach (var propSlider in propertySliders)
                {
                    FloatProperty prop = propSlider.prop;
                    if (prop == null) continue;

                    // 【核心逻辑】：通过具有身份的父级组件，向下抓取无名的 UI Slider 组件
                    UnityEngine.UI.Slider uiSlider = propSlider.GetComponentInChildren<UnityEngine.UI.Slider>(true);

                    if (uiSlider == null) continue;

                    // ---------------- 1. 对应 Penetration 参数 ----------------
                    if (prop.Name == "Penetration" || prop.name == "Penetration")
                    {
                        // 修改底层游戏逻辑最大值
                        if (prop.Max != NewMaxPenetration)
                        {
                            prop.Max = NewMaxPenetration;
                        }

                        // 修改 UI 面板上的 Slider 拖动上限 (Unity 暴漏的属性是 maxValue/minValue)
                        if (uiSlider.maxValue != SliderNewMaxPenetration)
                        {
                            uiSlider.maxValue = SliderNewMaxPenetration;
                        }
                    }
                    // ---------------- 2. 对应 Caliber 参数 ----------------
                    else if (prop.Name == "Caliber" || prop.name == "Caliber")
                    {
                        // 指正：修复了你原代码中复制粘贴导致的 Caliber 变量引用错误
                        if (prop.Min != NewMinCaliber) prop.Min = NewMinCaliber;
                        if (prop.Max != NewMaxCaliber) prop.Max = NewMaxCaliber;

                        // 修改 UI 面板上的 Slider 拖动下限和上限
                        if (uiSlider.minValue != SliderNewMinCaliber)
                        {
                            uiSlider.minValue = SliderNewMinCaliber;
                        }
                        if (uiSlider.maxValue != SliderNewMaxCaliber)
                        {
                            uiSlider.maxValue = SliderNewMaxCaliber;
                        }
                    }
                }

                // 0.1s 的检查频率
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}