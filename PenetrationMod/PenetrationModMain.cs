using System;
using System.Collections;
using MelonLoader;
using SprocketModAPI;
using UnityEngine;
using Il2CppProperties;
using Il2CppDynamicGUI;

[assembly: MelonInfo(typeof(PenetrationMod.PenetrationModMain), "Penetration Limit Modifier", "1.2.1", "furryAxw")]
[assembly: MelonGame("HD", "Sprocket")]
[assembly: MelonAdditionalDependencies("SprocketModAPI")]
[assembly: System.Reflection.AssemblyMetadata("Sprocket.Mod.Id", "furryaxw.penetration-mod")]
[assembly: System.Reflection.AssemblyMetadata("Sprocket.Mod.DisplayName", "Penetration Limit Modifier")]
[assembly: System.Reflection.AssemblyMetadata("Sprocket.Mod.Description", "Raises the caliber and penetration slider limits for the armor testing tool in the designer.")]
[assembly: System.Reflection.AssemblyMetadata("Sprocket.Mod.Authors", "furryAxw")]
[assembly: System.Reflection.AssemblyMetadata("Sprocket.Mod.Repository", "furryaxw/PenetrationMod")]
[assembly: System.Reflection.AssemblyMetadata("Sprocket.Mod.Category", "gameplay")]
[assembly: System.Reflection.AssemblyMetadata("Sprocket.Mod.License", "GPL-3.0-only")]

namespace PenetrationMod
{
    public class PenetrationModMain : MelonMod
    {
        private const string LimitsSection = "limits";
        private const string MaxPenetrationKey = "max-penetration";
        private const string SliderMaxPenetrationKey = "penetration-slider-max";
        private const string MinCaliberKey = "min-caliber";
        private const string MaxCaliberKey = "max-caliber";
        private const string SliderMaxCaliberKey = "caliber-slider-max";

        // 服务不可用（或配置页注册失败）时使用这些内置默认值。
        private const float DefaultMaxPenetration = 20000f;
        private const float DefaultSliderMaxPenetration = 1000f;
        private const float DefaultMinCaliber = 1f;
        private const float DefaultMaxCaliber = 5000f;
        private const float DefaultSliderMaxCaliber = 500f;

        private IModConfigService? configService;
        private IModConfigRegistration? configPage;

        // 用于记录当前是否在设计器模式中
        private bool isInDesigner = false;
        // 用于保存协程引用，方便在退出场景时立刻停止它
        private object? monitorCoroutine;

        // 逻辑极限值（由 API 自己的配置页驱动，改一项立刻生效）
        private float NewMaxPenetration = DefaultMaxPenetration;
        private float NewMinCaliber = DefaultMinCaliber;
        private float NewMaxCaliber = DefaultMaxCaliber;

        // UI 拖动条极限值
        private float SliderNewMaxPenetration = DefaultSliderMaxPenetration;
        private float SliderNewMinCaliber = DefaultMinCaliber;
        private float SliderNewMaxCaliber = DefaultSliderMaxCaliber;

        public override void OnInitializeMelon()
        {
            RegisterConfigPage();
            LoggerInstance.Msg($"[PLM] ready: penetration<={NewMaxPenetration} (slider {SliderNewMaxPenetration}), caliber {NewMinCaliber}..{NewMaxCaliber} (slider {SliderNewMinCaliber}..{SliderNewMaxCaliber})");
        }

        public override void OnDeinitializeMelon()
        {
            if (configService != null)
                configService.Changed -= OnConfigChanged;

            configPage?.Dispose();
            configPage = null;
            configService = null;
        }

        private void RegisterConfigPage()
        {
            if (!SprocketApi.TryGetService<IModConfigService>(out IModConfigService? config) || config == null)
            {
                LoggerInstance.Warning("[PLM] SprocketModAPI config service is unavailable; the built-in limits are used.");
                return;
            }

            try
            {
                configPage = config.Register(new ModConfigDefinition
                {
                    DisplayName = "Penetration Limit Modifier",
                    Sections = new[]
                    {
                        new ModConfigSectionDefinition
                        {
                            Id = LimitsSection,
                            Title = "Limits",
                            Description = "Vehicle designer limits. The game-logic cap is what designs are clamped to; the slider cap only sets how far the designer's slider can be dragged."
                        }
                    },
                    Entries = new[]
                    {
                        ModConfigEntryDefinition.Slider(MaxPenetrationKey, "Max penetration", DefaultMaxPenetration, 1000d, 1000000d, 1000d,
                            "Game-logic penetration cap.", LimitsSection),
                        ModConfigEntryDefinition.Slider(SliderMaxPenetrationKey, "Penetration slider cap", DefaultSliderMaxPenetration, 100d, 20000d, 100d,
                            "Upper end of the designer's penetration slider.", LimitsSection),
                        ModConfigEntryDefinition.Slider(MinCaliberKey, "Min caliber", DefaultMinCaliber, 0.1d, 1000d, 0.1d,
                            "Game-logic minimum caliber; also the lower end of the caliber slider.", LimitsSection),
                        ModConfigEntryDefinition.Slider(MaxCaliberKey, "Max caliber", DefaultMaxCaliber, 500d, 100000d, 500d,
                            "Game-logic maximum caliber.", LimitsSection),
                        ModConfigEntryDefinition.Slider(SliderMaxCaliberKey, "Caliber slider cap", DefaultSliderMaxCaliber, 100d, 10000d, 100d,
                            "Upper end of the designer's caliber slider.", LimitsSection),
                    },
                });

                configService = config;
                config.Changed += OnConfigChanged;
                Refresh();
                LoggerInstance.Msg($"[PLM] config page registered entries={configPage.Snapshot.Entries.Count}");
            }
            catch (Exception exception)
            {
                LoggerInstance.Warning($"[PLM] config registration failed: {exception.Message}");
                configPage = null;
            }
        }

        private void OnConfigChanged(ModConfigChangedEventArgs args)
        {
            if (configPage != null && args.ModId == configPage.Snapshot.ModId)
                Refresh();
        }

        private void Refresh()
        {
            IModConfigRegistration? page = configPage;
            if (page == null)
                return;

            NewMaxPenetration = (float)page.GetNumber(MaxPenetrationKey);
            SliderNewMaxPenetration = (float)page.GetNumber(SliderMaxPenetrationKey);
            NewMinCaliber = (float)page.GetNumber(MinCaliberKey);
            NewMaxCaliber = (float)page.GetNumber(MaxCaliberKey);
            SliderNewMinCaliber = NewMinCaliber;
            SliderNewMaxCaliber = (float)page.GetNumber(SliderMaxCaliberKey);
        }

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
            GameObject? configPanel = null;

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

                // 按 prop.Name 判定目标滑块，避免依赖滑块在层级中的顺序。
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
