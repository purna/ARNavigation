using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARNavigation.UI
{
    /// <summary>
    /// Manages navigation between different UI screens.
    /// Handles showing/hiding panels and animating transitions.
    /// </summary>
    public class NavigationController : MonoBehaviour
    {
        [Header("Screen References")]
        [SerializeField] private UIDocument start1Screen;
        [SerializeField] private UIDocument start2Screen;
        [SerializeField] private UIDocument start3Screen;
        [SerializeField] private UIDocument searchScreen;

        [Header("Settings")]
        [SerializeField] private float slideAnimationDuration = 0.3f;

        private readonly Stack<ScreenType> _history = new();
        private ScreenType _currentScreen = ScreenType.None;
        
        public enum ScreenType
        {
            None,
            Start1,
            Start2,
            Start3,
            Search
        }

        private void Start()
        {
            // Initialize all screens as hidden except Start1
            HideAllScreens();
            ShowScreen(ScreenType.Start1);
        }

        /// <summary>
        /// Navigate to a specific screen.
        /// </summary>
        public void NavigateTo(ScreenType screen)
        {
            if (_currentScreen != ScreenType.None && _currentScreen != screen)
            {
                _history.Push(_currentScreen);
            }
            
            ShowScreen(screen);
        }

        /// <summary>
        /// Navigate back to the previous screen.
        /// </summary>
        public void NavigateBack()
        {
            if (_history.Count > 0)
            {
                var previousScreen = _history.Pop();
                ShowScreen(previousScreen);
            }
            else
            {
                // Default behavior when no history - go to Start1
                ShowScreen(ScreenType.Start1);
            }
        }

        /// <summary>
        /// Get the current active screen.
        /// </summary>
        public ScreenType CurrentScreen => _currentScreen;

        private void ShowScreen(ScreenType screen)
        {
            HideAllScreens();
            
            switch (screen)
            {
                case ScreenType.Start1:
                    if (start1Screen != null)
                        start1Screen.rootVisualElement.style.display = DisplayStyle.Flex;
                    break;
                    
                case ScreenType.Start2:
                    if (start2Screen != null)
                        start2Screen.rootVisualElement.style.display = DisplayStyle.Flex;
                    break;
                    
                case ScreenType.Start3:
                    if (start3Screen != null)
                        start3Screen.rootVisualElement.style.display = DisplayStyle.Flex;
                    break;
                    
                case ScreenType.Search:
                    if (searchScreen != null)
                        searchScreen.rootVisualElement.style.display = DisplayStyle.Flex;
                    break;
            }
            
            _currentScreen = screen;
        }

        private void HideAllScreens()
        {
            if (start1Screen != null)
                start1Screen.rootVisualElement.style.display = DisplayStyle.None;
                
            if (start2Screen != null)
                start2Screen.rootVisualElement.style.display = DisplayStyle.None;
                
            if (start3Screen != null)
                start3Screen.rootVisualElement.style.display = DisplayStyle.None;
                
            if (searchScreen != null)
                searchScreen.rootVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Toggle the search panel visibility (slide up/down).
        /// </summary>
        public void ToggleSearchPanel(VisualElement searchPanel, VisualElement searchHeader)
        {
            if (searchPanel == null || searchHeader == null) return;
            
            bool isHidden = searchPanel.style.display == DisplayStyle.None || 
                           searchPanel.style.display == DisplayStyle.Flex && 
                           searchPanel.style.height.value.value == 0;
            
            if (isHidden)
            {
                // Show panel (slide up to 75% of screen)
                searchPanel.style.display = DisplayStyle.Flex;
                searchPanel.style.height = new StyleLength(new Length(75, LengthUnit.Percent));
            }
            else
            {
                // Hide panel (slide down)
                searchPanel.style.height = new StyleLength(new Length(0, LengthUnit.Percent));
                // Wait for animation before setting display to none
                StartCoroutine(HidePanelAfterDelay(searchPanel, slideAnimationDuration));
            }
        }

        private System.Collections.IEnumerator HidePanelAfterDelay(VisualElement panel, float delay)
        {
            yield return new WaitForSeconds(delay);
            panel.style.display = DisplayStyle.None;
        }
    }
}
