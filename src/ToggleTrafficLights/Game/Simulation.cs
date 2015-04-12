using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using Craxy.CitiesSkylines.ToggleTrafficLights.Game.UI.StateMachine;
using Craxy.CitiesSkylines.ToggleTrafficLights.Game.UI.StateMachine.States;
using Craxy.CitiesSkylines.ToggleTrafficLights.Utils;
using Craxy.CitiesSkylines.ToggleTrafficLights.Utils.Extensions;
using ICities;
using UnityEngine;

namespace Craxy.CitiesSkylines.ToggleTrafficLights.Game
{
    //Order of Events in Unity: https://i.imgur.com/NJage5W.png

    //C:S does not work with a class implementing two Interfaces at once:
    // it creates for each Interface one instance
    // therefore ILoadingExtension AND IThreadingExtension can not live together in the same instance

    public sealed class Loading : LoadingExtensionBase
    {
        #region Overrides of LoadingExtensionBase

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            Simulation.OnCreated(loading);
        }

        public override void OnReleased()
        {
            base.OnReleased();

            Simulation.OnReleased();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            Simulation.OnLevelLoaded(mode);
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            Simulation.OnLevelUnloading();
        }

        #endregion

        public Simulation Simulation
        {
            get { return SimulationInstance.Simulation; }
        }
    }

    public sealed class Threading : ThreadingExtensionBase
    {
        public Simulation Simulation
        {
            get { return SimulationInstance.Simulation; }
        }

        #region Overrides of ThreadingExtensionBase

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            base.OnUpdate(realTimeDelta, simulationTimeDelta);

//            Simulation.OnUpdate(realTimeDelta, simulationTimeDelta);

#if DEBUG
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
            {
                if (_btn == null)
                {
                    var roadsOptionPanel = UiHelper.FindComponent<UIComponent>("RoadsOptionPanel(RoadsPanel)", null, UiHelper.FindOptions.NameContains);
                    if (roadsOptionPanel == null || !roadsOptionPanel.gameObject.activeInHierarchy)
                    {
                        DebugLog.Info("RoadsOptionPanel not here");
                        return;
                    }

                    const int spriteWidth = 31;
                    const int spriteHeight = 31;

                    _btn = roadsOptionPanel.AddUIComponent<UIMultiStateButton>();
                    _btn.name = "MultiStateTrafficLightsButton";
                    _btn.tooltip = "Some test button";
                    _btn.size = new Vector2(spriteWidth, spriteHeight);


                    var atlas =ButtonStateBase.CreateAtlas("icons.png", "ToggleTrafficLightsUI",
                                    UIView.Find<UITabstrip>("ToolMode").atlas.material,
                                    spriteWidth, spriteHeight, new[]
                                                    {
                                                        "OptionBase",
                                                        "OptionBaseDisabled",
                                                        "OptionBaseFocused",
                                                        "OptionBaseHovered",
                                                        "OptionBasePressed",
                                                        "Selected",
                                                        "Unselected",
                                                        "OptionBaseFocusedRed",
                                                    });
                    _btn.atlas = atlas;
                    _btn.playAudioEvents = true;
                    _btn.relativePosition = new Vector3(131, 38);


                    {
                        var bss = _btn.backgroundSprites;
                        bss.Clear();
                        bss.AddState();

                        var bs1 = bss[0];
                        var bs2 = bss[1];

                        bs1.normal = "OptionBase";
                        bs1.disabled = "OptionBase";
                        bs1.hovered = "OptionBaseHovered";
                        bs1.pressed = "OptionBasePressed";
                        bs1.focused = "OptionBase";

                        bs2.normal = "OptionBaseFocused";
                        bs2.disabled = "OptionBaseFocused";
                        bs2.hovered = "OptionBaseFocused";
                        bs2.pressed = "OptionBaseFocused";
                        bs2.focused = "OptionBaseFocused";
                    }


                    {
                        var fss = _btn.foregroundSprites;
                        fss.Clear();
                        fss.AddState();

                        var fs1 = fss[0];
                        var fs2 = fss[1];

                        fs1.normal = "Unselected";
                        fs1.disabled = "Unselected";
                        fs1.hovered = "Unselected";
                        fs1.pressed = "Unselected";
                        fs1.focused = "Unselected";

                        fs2.normal = "Selected";
                        fs2.disabled = "Selected";
                        fs2.hovered = "Selected";
                        fs2.pressed = "Selected";
                        fs2.focused = "Selected";
                    }

                    //This does not work -> Throws null reference error (GetForegroundRenderOffset)
                    //BUT: when setting value via Mod Tools it suddenly works
                    _btn.activeStateIndex = 0;

                    _btn.eventClick += (component, param) =>
                    {
                        DebugLog.Info("Clicked. Active State Index: {0}", _btn.activeStateIndex);
//                        if (_btn.activeStateIndex == 0)
//                        {
//                            _btn.activeStateIndex = 1;
//                        }
//                        else
//                        {
//                            _btn.activeStateIndex = 0;
//                        }
                    };
                }
            }
#endif
        }


#if DEBUG
        private UIMultiStateButton _btn = null;
#endif

        #endregion
    }

    // I don't want to add an static Instance property to the Simulation class...
    public static class SimulationInstance
    {
        public static readonly Simulation Simulation = new Simulation();
    }

    public sealed class Simulation
    {
        #region members
        private TrafficLightsMachine _stateMachine = null;
        #endregion

        #region properties
        public ILoading LoadingManager { get; private set; }
        public IManagers Managers
        {
            get
            {
                return this.LoadingManager.managers;
            }
        }

        #endregion

        #region Implementation of IThreadingExtension

        public void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (IsLoading() || !IsGameMode())
            {
                return;
            }

#if DEBUG
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
            {
                DebugLog.Info("Current State: {0}", _stateMachine.CurrentState);
                DebugLog.Info("Current tool: {0}", ToolsModifierControl.toolController.CurrentTool);
            }
#endif

            _stateMachine.OnUpdate();
        }

        #endregion

        #region Overrides of LoadingExtensionBase

        public void OnCreated(ILoading loading)
        {
            LoadingManager = loading;

            DebugLog.Message("Created v.{0} at {1}", Assembly.GetExecutingAssembly().GetName().Version, DateTime.Now);
        }

        public void OnReleased()
        {
            if (_stateMachine != null)
            {
                _stateMachine.Destroy();
            }
            _stateMachine = null;
                        
            DebugLog.Message("Released v.{0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        public void OnLevelLoaded(LoadMode mode)
        {

            if (IsGameMode())
            {
                _stateMachine = new TrafficLightsMachine();
                DebugLog.Message("Level loaded");
            }
            else
            {
                DebugLog.Message("In Editor -> mod is disabled");
            }
        }

        public void OnLevelUnloading()
        {
            if (_stateMachine != null)
            {
                _stateMachine.Destroy();
            }
            _stateMachine = null;
            DebugLog.Message("Level unloaded");
        }

        #endregion

        #region helpers
        private bool IsGameMode()
        {
            if (LoadingManager != null)
            {
                return LoadingManager.IsGameMode();
            }
            //don't know -> go on
            DebugLog.Warning("IsGameMode: unknown -- default to true");
            var st = new StackTrace();
            DebugLog.Warning(st.ToString());
            return true;
        }

        private bool IsLoading()
        {
            return _stateMachine == null;
        }
        #endregion

    }
}