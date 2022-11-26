using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Taco.Classes
{
    public class RenderingSplitContainer : SplitContainer
    {
        private bool m_bDragRenderEnabled = true;

        public bool DragRenderEnabled
        {
            get { return m_bDragRenderEnabled; }
            set { EnableDragRender(value); }
        }

        public RenderingSplitContainer()
        {
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
        }

        public void EnableDragRender(bool bEnable)
        {
            if (bEnable && !m_bDragRenderEnabled)
            {
                MouseDown += OnMouseDown;
                MouseUp += OnMouseUp;
                MouseMove += OnMouseMove;
                m_bDragRenderEnabled = true;
            }
            else if (!bEnable && m_bDragRenderEnabled)
            {
                MouseDown -= OnMouseDown;
                MouseUp -= OnMouseUp;
                MouseMove -= OnMouseMove;
                m_bDragRenderEnabled = false;
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            ((SplitContainer)sender).IsSplitterFixed = true;
        }
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            ((SplitContainer)sender).IsSplitterFixed = false;
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // Check to make sure the splitter won't be updated by the
            // normal move behavior also
            if (((SplitContainer)sender).IsSplitterFixed)
            {
                // Make sure that the button used to move the splitter
                // is the left mouse button
                if (e.Button.Equals(MouseButtons.Left))
                {
                    // Checks to see if the splitter is aligned Vertically
                    if (((SplitContainer)sender).Orientation.Equals(Orientation.Vertical))
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.X > 0 && e.X < ((SplitContainer)sender).Width)
                        {
                            // Move the splitter & force a visual refresh
                            ((SplitContainer)sender).SplitterDistance = e.X;
                            ((SplitContainer)sender).Refresh();
                        }
                    }
                    // If it isn't aligned vertically then it must be
                    // horizontal
                    else
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.Y > 0 && e.Y < ((SplitContainer)sender).Height)
                        {
                            // Move the splitter & force a visual refresh
                            ((SplitContainer)sender).SplitterDistance = e.Y;
                            ((SplitContainer)sender).Refresh();
                        }
                    }
                }
                // If a button other than left is pressed or no button
                // at all
                else
                {
                    // This allows the splitter to be moved normally again
                    ((SplitContainer)sender).IsSplitterFixed = false;
                }
            }
        }
    }
}
