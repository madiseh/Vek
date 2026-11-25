using System;
using Microsoft.AspNetCore.Components.Web;

namespace Vek
{
    /// <summary>
    /// Coordinates touch and pointer swipe gesture tracking and dragging calculations for <see cref="VekDrawer"/>.
    /// </summary>
    public class DrawerSwipeHandler
    {
        private double? _startX;
        private double? _startY;
        private double? _currentX;
        private double? _currentY;
        private bool _isTracking;
        private bool _isDragging;

        /// <summary>
        /// Gets or sets the threshold in pixels required to trigger an open or close swipe.
        /// </summary>
        public double Threshold { get; set; } = 80;

        /// <summary>
        /// Gets or sets the edge hit-target zone width in pixels from the screen or container boundary
        /// within which a swipe gesture to open the drawer must start.
        /// </summary>
        public double EdgeWidth { get; set; } = 40;

        /// <summary>
        /// Gets or sets whether touch gestures are enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets whether a swipe gesture is currently active (finger down and tracking).
        /// </summary>
        public bool IsTracking => _isTracking;

        /// <summary>
        /// Gets whether dragging has exceeded the horizontal movement threshold and is actively dragging.
        /// </summary>
        public bool IsDragging => _isDragging;

        /// <summary>
        /// Gets the current drag displacement offset in pixels along the active axis.
        /// Negative values indicate dragging towards closing for start/left drawers, positive for opening.
        /// </summary>
        public double DragOffset { get; private set; }

        /// <summary>
        /// Handles pointer down event. Checks whether the touch started within the edge area (if drawer is closed)
        /// or anywhere on the drawer surface (if drawer is open).
        /// </summary>
        /// <param name="clientX">The client X coordinate.</param>
        /// <param name="clientY">The client Y coordinate.</param>
        /// <param name="isOpen">Whether the drawer is currently open.</param>
        /// <param name="anchor">The anchor position of the drawer.</param>
        /// <param name="containerWidth">The container width, or 0 if unknown.</param>
        /// <returns><c>true</c> if tracking began; otherwise, <c>false</c>.</returns>
        public bool OnPointerDown(double clientX, double clientY, bool isOpen, Anchor anchor, double containerWidth = 0)
        {
            if (!Enabled)
            {
                return false;
            }

            // If the drawer is closed, only start tracking if pointer is near the relevant edge
            if (!isOpen)
            {
                if (!IsNearEdge(clientX, anchor, containerWidth))
                {
                    return false;
                }
            }

            _startX = clientX;
            _startY = clientY;
            _currentX = clientX;
            _currentY = clientY;
            _isTracking = true;
            _isDragging = false;
            DragOffset = 0;
            return true;
        }

        /// <summary>
        /// Handles pointer move event and calculates real-time drag offsets.
        /// </summary>
        /// <param name="clientX">The client X coordinate.</param>
        /// <param name="clientY">The client Y coordinate.</param>
        /// <param name="isOpen">Whether the drawer is currently open.</param>
        /// <param name="anchor">The anchor position of the drawer.</param>
        /// <returns><c>true</c> if drag offset changed and UI should update; otherwise, <c>false</c>.</returns>
        public bool OnPointerMove(double clientX, double clientY, bool isOpen, Anchor anchor)
        {
            if (!_isTracking || _startX is null || _startY is null)
            {
                return false;
            }

            _currentX = clientX;
            _currentY = clientY;

            var deltaX = clientX - _startX.Value;
            var deltaY = clientY - _startY.Value;

            // If vertical movement exceeds horizontal before dragging starts, abort gesture to allow natural scrolling
            if (!_isDragging && Math.Abs(deltaY) > Math.Abs(deltaX) && Math.Abs(deltaY) > 10)
            {
                Reset();
                return false;
            }

            if (!_isDragging && Math.Abs(deltaX) > 8)
            {
                _isDragging = true;
            }

            if (_isDragging)
            {
                // Calculate drag offset based on anchor position
                // For Anchor.Start / Left:
                // When open: swiping left (negative deltaX) closes drawer. Clamp offset between -drawerWidth and 0.
                // When closed: swiping right (positive deltaX) opens drawer. Clamp offset between 0 and drawerWidth.
                if (anchor is Anchor.Start or Anchor.Left)
                {
                    DragOffset = isOpen ? Math.Min(0, deltaX) : Math.Max(0, deltaX);
                }
                else if (anchor is Anchor.End or Anchor.Right)
                {
                    // For Anchor.End / Right:
                    // When open: swiping right (positive deltaX) closes drawer.
                    // When closed: swiping left (negative deltaX) opens drawer.
                    DragOffset = isOpen ? Math.Max(0, deltaX) : Math.Min(0, deltaX);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles pointer up event and evaluates whether the gesture exceeds the threshold to open or close the drawer.
        /// </summary>
        /// <param name="clientX">The client X coordinate.</param>
        /// <param name="clientY">The client Y coordinate.</param>
        /// <param name="isOpen">Whether the drawer was open at gesture start.</param>
        /// <param name="anchor">The drawer anchor position.</param>
        /// <returns>A tuple with <c>Handled</c> indicating whether a swipe action completed and <c>NewOpenState</c> indicating the resulting open state.</returns>
        public (bool Handled, bool NewOpenState) OnPointerUp(double clientX, double clientY, bool isOpen, Anchor anchor)
        {
            if (!_isTracking || _startX is null)
            {
                Reset();
                return (false, isOpen);
            }

            var deltaX = clientX - _startX.Value;
            var deltaY = _startY.HasValue ? clientY - _startY.Value : 0;
            var isHorizontal = Math.Abs(deltaX) > Math.Abs(deltaY);

            bool newOpenState = isOpen;
            bool handled = false;

            if (isHorizontal && Math.Abs(deltaX) >= Threshold)
            {
                if (anchor is Anchor.Start or Anchor.Left)
                {
                    if (isOpen && deltaX <= -Threshold)
                    {
                        // Swiped left while open -> close
                        newOpenState = false;
                        handled = true;
                    }
                    else if (!isOpen && deltaX >= Threshold)
                    {
                        // Swiped right while closed -> open
                        newOpenState = true;
                        handled = true;
                    }
                }
                else if (anchor is Anchor.End or Anchor.Right)
                {
                    if (isOpen && deltaX >= Threshold)
                    {
                        // Swiped right while open -> close
                        newOpenState = false;
                        handled = true;
                    }
                    else if (!isOpen && deltaX <= -Threshold)
                    {
                        // Swiped left while closed -> open
                        newOpenState = true;
                        handled = true;
                    }
                }
            }

            Reset();
            return (handled, newOpenState);
        }

        /// <summary>
        /// Resets active gesture tracking state.
        /// </summary>
        public void Reset()
        {
            _startX = null;
            _startY = null;
            _currentX = null;
            _currentY = null;
            _isTracking = false;
            _isDragging = false;
            DragOffset = 0;
        }

        private bool IsNearEdge(double clientX, Anchor anchor, double containerWidth)
        {
            if (anchor is Anchor.Start or Anchor.Left)
            {
                return clientX <= EdgeWidth;
            }

            if (anchor is Anchor.End or Anchor.Right)
            {
                if (containerWidth > 0)
                {
                    return clientX >= (containerWidth - EdgeWidth);
                }

                // If container width is not explicitly known, we accept coordinates where clientX > 0
                return true;
            }

            return false;
        }
    }
}
