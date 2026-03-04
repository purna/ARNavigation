using UnityEngine;
using UnityEngine.UIElements;

namespace ARNavigation.UI
{
    /// <summary>
    /// Controller for the Start3 (Room Number Entry) screen.
    /// Handles room number input and navigation back.
    /// </summary>
    public class Start3Screen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private NavigationController navigationController;

        private Button _backButton;
        private Button _confirmButton;
        private TextField _roomInput;

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                
                // Get buttons
                _backButton = root.Q<Button>("back-button");
                _confirmButton = root.Q<Button>("confirm-button");
                _roomInput = root.Q<TextField>("room-input");
                
                // Register click handlers
                if (_backButton != null)
                {
                    _backButton.clicked += OnBackButtonClicked;
                }
                
                if (_confirmButton != null)
                {
                    _confirmButton.clicked += OnConfirmButtonClicked;
                }
            }
        }

        private void OnDisable()
        {
            // Unregister click handlers
            if (_backButton != null)
            {
                _backButton.clicked -= OnBackButtonClicked;
            }
            
            if (_confirmButton != null)
            {
                _confirmButton.clicked -= OnConfirmButtonClicked;
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
        /// Called when confirm button is clicked.
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            string roomNumber = "";
            
            if (_roomInput != null)
            {
                roomNumber = _roomInput.value;
            }
            
            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                Debug.LogWarning("Please enter a room number");
                // TODO: Show error message to user
                return;
            }
            
            Debug.Log($"Room number entered: {roomNumber}");
            
            // TODO: Process room number and navigate to navigation
            // For now, we'll navigate to search screen
            if (navigationController != null)
            {
                navigationController.NavigateTo(NavigationController.ScreenType.Search);
            }
        }

        /// <summary>
        /// Called when the user submits the room number via keyboard.
        /// </summary>
        private void OnRoomInputSubmit(string value)
        {
            OnConfirmButtonClicked();
        }
    }
}
