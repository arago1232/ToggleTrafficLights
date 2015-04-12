using ColossalFramework;
using ColossalFramework.UI;
using Craxy.CitiesSkylines.ToggleTrafficLights.Utils;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Craxy.CitiesSkylines.ToggleTrafficLights.Game.UI.Menu
{
    public class BatchPanel
    {
        #region fields
        private EasingType _showEasingType = EasingType.ExpoEaseOut;
        private EasingType _hideEasingType = EasingType.ExpoEaseOut;
        private PoliciesPanel.DockingPosition _dockingPosition = PoliciesPanel.DockingPosition.Right;

        #region UI

        private UIPanel _panel = null;
        #endregion

        #endregion

        #region properties
        public bool IsVisible
        {
            get { return _panel != null && _panel.isVisible; }
        }
        #endregion

        #region UI

        public void Show()
        {
            if (_panel == null)
            {
                Setup();
            }

            DebugLog.Info("BatchPanel: Show");

            Debug.Assert(_panel != null, "_panel != null");
            _panel.Show(true);
        }

        public void Hide()
        {
            DebugLog.Info("BatchPanel: Hide");

            if (_panel != null)
            {
                _panel.Hide();
            }
        }
        public void Setup()
        {
            DebugLog.Info("Setup");

            SetupPanel();
            SetupControls();
        }

        private void SetupPanel()
        {
            //From city policies
            var template = GetPoliciesUiPanel();

            //FullScreenContainer
            var root = template.parent;

            _panel = root.AddUIComponent<UIPanel>();
            _panel.name = "ToggleTrafficLightsBatchPanel";
            _panel.backgroundSprite = template.backgroundSprite;
//            _panel.color = template.color;
            _panel.width = template.width;
            _panel.height = template.height;
            _panel.relativePosition = new Vector3(_panel.parent.relativePosition.x, _panel.parent.relativePosition.y + _panel.parent.size.y - _panel.size.y);
            _panel.padding = template.padding;

            _panel.isVisible = false;
        }
        private void SetupControls()
        {
            SetupCaption();
        }

        private void SetupCaption()
        {
            var template = GetPoliciesPanel().Find<UILabel>("Caption");
            var label = _panel.AddUIComponent<UILabel>();
            label.name = "ToggleTrafficLightsBatchPanelCaption";
            label.text = "Toggle Traffic Lights";
            label.size = template.size;
            label.backgroundSprite = template.backgroundSprite;
            label.relativePosition = template.relativePosition;
            label.padding = template.padding;
            label.font = template.font;
            label.textColor = template.textColor;
            label.pivot = template.pivot;
            label.textAlignment = template.textAlignment;
            label.verticalAlignment = template.verticalAlignment;
        }

        #endregion


        #region policies
        private PoliciesPanel GetPoliciesPanel()
        {
            return Object.FindObjectOfType<PoliciesPanel>();
        }

        private UIPanel GetPoliciesUiPanel()
        {
            return (UIPanel)UIView.Find("PoliciesPanel");
        }
        #endregion
    }
}