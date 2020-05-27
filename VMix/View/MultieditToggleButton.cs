using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace VMix
{
    public class MultieditToggleButton : ToggleButton
    {
        public static readonly DependencyProperty MultipleDataProperty = DependencyProperty.Register("MultipleData", typeof(bool), typeof(MultieditToggleButton));
        public static readonly DependencyProperty DragToggleButtonProperty = DependencyProperty.Register("DragToggleButton", typeof(bool), typeof(MultieditToggleButton));

        public bool MultipleData
        {
            get { return (bool)GetValue(MultipleDataProperty); }
            set { SetValue(MultipleDataProperty, value); }
        }
        public bool DragToggleButton
        {
            get { return (bool)GetValue(DragToggleButtonProperty); }
            set { SetValue(DragToggleButtonProperty, value); }
        }

        public static void OnMultipleDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            /*sender.GetValue(ContentProperty);
            if ((bool)sender.GetValue(MultipleDataProperty))
            {
                ((ToggleButton)sender).SetResourceReference(BackgroundProperty, "ControlFillMixedValues");
            } else
            {
                //Reset it to the template value
                //((ToggleButton)sender).SetResourceReference(BackgroundProperty, "");
                BindingOperations.ClearBinding(sender, BackgroundProperty);
            }*/
        }

        #region Drag toggle button implementation
        /// <summary>
        /// This is the method that responds to the MouseButtonEvent event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!DragToggleButton)
            {
                base.OnMouseLeftButtonDown(e);
                return;
            }

            // Ignore when in hover-click mode.
            if (ClickMode != ClickMode.Hover)
            {
                e.Handled = true;

                // Always set focus on itself
                // In case ButtonBase is inside a nested focus scope we should restore the focus OnLostMouseCapture
                Focus();

                // It is possible that the mouse state could have changed during all of
                // the call-outs that have happened so far.
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    //IsPressed = !IsPressed;
                }

                if (ClickMode == ClickMode.Release)
                {
                    bool exceptionThrown = true;
                    try
                    {
                        OnClick();
                        exceptionThrown = false;
                    }
                    finally
                    {
                        if (exceptionThrown)
                        {
                            // Cleanup the buttonbase state
                            IsPressed = false;
                            ReleaseMouseCapture();
                        }
                    }
                }
            }

            //base.OnMouseLeftButtonDown(e);
        }

        /// <summary>
        ///     Called when this element loses mouse capture.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            if (!DragToggleButton)
            {
                base.OnLostMouseCapture(e);
                return;
            }
        }

        /// <summary>
        ///     An event reporting the mouse entered this element.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (!DragToggleButton)
            {
                base.OnMouseEnter(e);
                return;
            }

            //Allows for drag pressing
            if (e.LeftButton == MouseButtonState.Pressed)
                OnClick();

            base.OnMouseEnter(e);
            if (HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        /// <summary>
        ///     An event reporting the mouse left this element.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!DragToggleButton)
            {
                base.OnMouseLeave(e);
                return;
            }

            base.OnMouseLeave(e);
            if (HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        /// <summary>
        ///     An event reporting that the IsMouseOver property changed.
        /// </summary>
        private bool HandleIsMouseOverChanged()
        {
            return false;
        }
        #endregion
    }
}
