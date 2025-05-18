using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Mica.Behaviors
{
    public static class TextBoxBehaviors
    {
        // 1. Define the attached property to enable the behavior
        public static readonly DependencyProperty AutoAdvanceOnTextChangeProperty =
            DependencyProperty.RegisterAttached(
                "AutoAdvanceOnTextChange",
                typeof(bool),
                typeof(TextBoxBehaviors),
                new PropertyMetadata(false, OnAutoAdvanceOnTextChangeChanged));

        // Getter for the attached property
        public static bool GetAutoAdvanceOnTextChange(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoAdvanceOnTextChangeProperty);
        }

        // Setter for the attached property
        public static void SetAutoAdvanceOnTextChange(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoAdvanceOnTextChangeProperty, value);
        }

        // 2. Property changed callback - triggered when the attached property's value changes
        private static void OnAutoAdvanceOnTextChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Check if the attached property is applied to a TextBox
            if (d is TextBox textBox)
            {
                bool newValue = (bool)e.NewValue;
                if (newValue)
                {
                    // If enabling the behavior, subscribe to necessary events
                    textBox.TextChanged += TextBox_TextChanged;
                    // Optional: Subscribe to PreviewTextInput here if you want the behavior to handle digit-only input filtering as well
                    textBox.PreviewTextInput += TextBox_PreviewTextInput;
                    // Optional: Subscribe to KeyDown for backspace focus movement here
                    textBox.KeyDown += TextBox_KeyDown;
                }
                else
                {
                    // If disabling the behavior, unsubscribe from events (important for preventing memory leaks)
                    textBox.TextChanged -= TextBox_TextChanged;
                    textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                    textBox.KeyDown -= TextBox_KeyDown;
                }
            }
        }

        // 3. Event handler for PreviewTextInput (Optional - handles digit filtering)
        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digit input (you can also add logic for paste here if needed)
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true; // Prevent the non-digit input
            }
        }

        // 4. Event handler for KeyDown (Optional - handles backspace focus movement)
        private static void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            // If Backspace is pressed AND the current textbox is empty, move focus to the previous one
            if (e.Key == Key.Back && string.IsNullOrEmpty(textBox.Text))
            {
                // Find the previous focusable element in the tab order
                TraversalRequest request = new(FocusNavigationDirection.Previous);
                // Use Dispatcher to ensure focus change happens after the UI processes the key press
                _ = textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (textBox.MoveFocus(request))
                    {
                        e.Handled = true; // Mark event as handled if focus successfully moved
                                          // Optional: Set caret at the end of the previous textbox
                        if (FocusManager.GetFocusedElement(FocusManager.GetFocusScope(textBox)) is TextBox previousTextBox)
                        {
                            previousTextBox.CaretIndex = previousTextBox.Text.Length;
                        }
                    }
                }), DispatcherPriority.Render); // Use Render priority for UI updates
            }
        }

        // 5. Event handler for TextChanged - the core logic for advancing focus
        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            // If the textbox now contains exactly one character (due to MaxLength="1" and PreviewTextInput)
            if (textBox.Text.Length == 1)
            {
                // Find the next focusable element in the logical/visual tree's tab order
                TraversalRequest request = new(FocusNavigationDirection.Next);

                // Use Dispatcher to delay focusing until after the binding updates and UI is ready
                _ = textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _ = textBox.MoveFocus(request);
                    // Optional: Select all text in the next box for easy replacement
                    // if (FocusManager.GetFocusedElement(FocusManager.GetFocusScope(textBox)) is TextBox nextTextBox)
                    // {
                    //      nextTextBox.SelectAll();
                    // }

                    // If this was the last textbox and it's filled, you might want to trigger a command in the ViewModel.
                    // This would require a more complex behavior that takes a Command as an attached property,
                    // or using the Messenger in the ViewModel based on property changes.
                    // For just focus movement, this behavior is sufficient.

                }), DispatcherPriority.Render); // Use Render priority
            }
            // Optional: Handle clearing the box (e.g., if the user manually deletes the digit)
            // If the text length becomes 0, you might want to move focus back
            else if (textBox.Text.Length == 0)
            {
                // The KeyDown handler for Backspace covers the most common case.
                // This else-if might be needed if text is cleared by other means (e.g., programmatically or Ctrl+X)
                // If you want focus to go back when the box becomes empty *by any means*, uncomment and adjust.
                // var request = new TraversalRequest(FocusNavigationDirection.Previous);
                // textBox.Dispatcher.BeginInvoke(new Action(() =>
                // {
                //     textBox.MoveFocus(request);
                // }), DispatcherPriority.Render);
            }
        }

        // Helper to find the next element in the logical tree that is focusable (less reliable than TraversalRequest)
        // private static IInputElement? FindNextFocusableElement(DependencyObject current)
        // {
        //     bool foundCurrent = false;
        //     foreach (var element in FindFocusableElements(VisualTreeHelper.GetParent(current) as DependencyObject))
        //     {
        //         if (element == current)
        //         {
        //             foundCurrent = true;
        //             continue;
        //         }
        //         if (foundCurrent)
        //         {
        //             return element;
        //         }
        //     }
        //     return null;
        // }

        // Helper to find all focusable elements in a subtree (less reliable than TraversalRequest for 'next')
        // private static IEnumerable<IInputElement> FindFocusableElements(DependencyObject? parent)
        // {
        //     if (parent == null) yield break;

        //     if (parent is IInputElement inputElement && inputElement.Focusable && inputElement.IsEnabled && inputElement.IsVisible)
        //     {
        //         yield return inputElement;
        //     }

        //     for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        //     {
        //         var child = VisualTreeHelper.GetChild(parent, i) as DependencyObject;
        //         if (child != null)
        //         {
        //             foreach (var focusable in FindFocusableElements(child))
        //             {
        //                 yield return focusable;
        //             }
        //         }
        //     }
        // }
    }

}
