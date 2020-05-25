using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace VMix
{
    public class DragToggleButton : ToggleButton
    {
        /// <summary>
        /// This override method is called when the control is clicked by mouse or keyboard
        /// </summary>
        protected override void OnClick()
        {
            //Cycles the current toggle state
            //OnToggle();

            base.OnClick();
        }

        /// <summary>
        /// This is the method that responds to the MouseButtonEvent event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
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
        /// This is the method that responds to the MouseButtonEvent event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }

        //The following methods just ensure the base does nothing

        /// <summary>
        /// This is the method that responds to the MouseEvent event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        /// <summary>
        ///     Called when this element loses mouse capture.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
        }

        /// <summary>
        ///     An event reporting the mouse entered this element.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
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
    }
}
