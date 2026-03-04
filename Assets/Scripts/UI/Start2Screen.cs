using UnityEngine;
using UnityEngine.UIElements;

namespace ARNavigation.UI
{
    /// <summary>
    /// Controller for the Start2 (Location Selection) screen.
    /// Handles location selection and navigation to Start3 or Search.
    /// </summary>
    public class Start2Screen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private NavigationController navigationController;

        private Button _backButton;
        private Button _locOtherButton;
        
        // Location buttons
        private Button _mainEntranceButton;
        private Button _assemblyHallButton;
        private Button _canteenButton;
        private Button _libraryButton;

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                
                // Get navigation buttons
                _backButton = root.Q<Button>("back-button");
                _locOtherButton = root.Q<Button>("loc-other");
                
                // Get location buttons
                _mainEntranceButton = root.Q<Button>("loc-main-entrance");
                _assemblyHallButton = root.Q<Button>("loc-assembly-hall");
                _canteenButton = root.Q<Button>("loc-canteen");
                _libraryButton = root.Q<Button>("loc-library");
                
                // Register click handlers
                if (_backButton != null)
                {
                    _backButton.clicked += OnBackButtonClicked;
                }
                
                if (_locOtherButton != null)
                {
                    _locOtherButton.clicked += OnOtherButtonClicked;
                }
                
                // Location button handlers
                RegisterLocationButton(_mainEntranceButton, "Main Entrance");
                RegisterLocationButton(_assemblyHallButton, "Assembly Hall");
                RegisterLocationButton(_canteenButton, "Canteen");
                RegisterLocationButton(_libraryButton, "Library");
            }
        }

        private void OnDisable()
        {
            // Unregister click handlers
            if (_backButton != null)
                _backButton.clicked -= OnBackButtonClicked;
                
            if (_locOtherButton != null)
                _locOtherButton.clicked -= OnOtherButtonClicked;
                
            UnregisterLocationButton(_mainEntranceButton, "Main Entrance");
            UnregisterLocationButton(_assemblyHallButton, "Assembly Hall");
            UnregisterLocationButton(_canteenButton, "Canteen");
            UnregisterLocationButton(_libraryButton, "Library");
        }

        private void RegisterLocationButton(Button button, string locationName)
        {
            if (button != null)
            {
                button.clicked += () => OnLocationSelected(locationName);
            }
        }

        private void UnregisterLocationButton(Button button, string locationName)
        {
            if (button != null)
            {
                button.clicked -= () => OnLocationSelected(locationName);
            }
        }

        /// <summary>
        /// Called when back button is clicked.
        /// </summary>
        private void OnBackButtonClicked()
        {
            if (navigationController != null)
            {
                navigationController.NavigateBack();
            }
        }

        /// <summary>
        /// Called when "Other/Enter room number" button is clicked.
        /// </summary>
        private void OnOtherButtonClicked()
        {
            if (navigationController != null)
            {
                navigationController.NavigateTo(NavigationController.ScreenType.Start3);
            }
        }

        /// <summary>
        /// Called when a location button is selected.
        /// </summary>
        private void OnLocationSelected(string locationName)
        {
            Debug.Log($"Location selected: {locationName}");
            // TODO: Store selected location and proceed to navigation
            // For now, we'll show the search screen
            if (navigationController != null)
            {
                navigationController.NavigateTo(NavigationController.ScreenType.Search);
            }
        }
    }
}
