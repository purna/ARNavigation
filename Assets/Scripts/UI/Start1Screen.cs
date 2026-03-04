using UnityEngine;
using UnityEngine.UIElements;

namespace ARNavigation.UI
{
    /// <summary>
    /// Controller for the Start1 (QR Scan) screen.
    /// Handles navigation to Start2 and Search screens.
    /// </summary>
    public class Start1Screen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private NavigationController navigationController;

        private Button _manualButton;
        private Button _searchHeaderButton;
        private VisualElement _searchPanel;
        private bool _isSearchPanelVisible = false;

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                
                // Get buttons by name
                _manualButton = root.Q<Button>("manual-button");
                _searchHeaderButton = root.Q<Button>("search-header");
                _searchPanel = root.Q<VisualElement>("search-panel");
                
                // Register click handlers
                if (_manualButton != null)
                {
                    _manualButton.clicked += OnManualButtonClicked;
                }
                
                if (_searchHeaderButton != null)
                {
                    _searchHeaderButton.clicked += OnSearchHeaderClicked;
                }
            }
        }

        private void OnDisable()
        {
            // Unregister click handlers
            if (_manualButton != null)
            {
                _manualButton.clicked -= OnManualButtonClicked;
            }
            
            if (_searchHeaderButton != null)
            {
                _searchHeaderButton.clicked -= OnSearchHeaderClicked;
            }
        }

        /// <summary>
        /// Called when "Enter location manually" button is clicked.
        /// </summary>
        private void OnManualButtonClicked()
        {
            if (navigationController != null)
            {
                navigationController.NavigateTo(NavigationController.ScreenType.Start2);
            }
        }

        /// <summary>
        /// Called when search header button is clicked - toggles search panel.
        /// </summary>
        private void OnSearchHeaderClicked()
        {
            ToggleSearchPanel();
        }

        /// <summary>
        /// Toggle the search panel visibility (slides up to 75% of screen).
        /// </summary>
        private void ToggleSearchPanel()
        {
            if (_searchPanel == null) return;
            
            _isSearchPanelVisible = !_isSearchPanelVisible;
            
            if (_isSearchPanelVisible)
            {
                // Show panel - slide up to 75%
                _searchPanel.style.display = DisplayStyle.Flex;
                _searchPanel.schedule.Execute(() =>
                {
                    _searchPanel.AddToClassList("visible");
                });
            }
            else
            {
                // Hide panel - slide down
                _searchPanel.RemoveFromClassList("visible");
                _searchPanel.schedule.Execute(() =>
                {
                    if (!_isSearchPanelVisible)
                    {
                        _searchPanel.style.display = DisplayStyle.None;
                    }
                }).StartingIn(300);
            }
        }

        /// <summary>
        /// Called when QR scan is successful.
        /// </summary>
        public void OnQRScanSuccess(string locationData)
        {
            Debug.Log($"QR Code scanned: {locationData}");
            // TODO: Parse location data and navigate accordingly
        }
    }
}
