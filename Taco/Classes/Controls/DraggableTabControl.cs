using System.Drawing;
using System.Windows.Forms;

namespace Taco.Classes
{
    public class DraggableTabControl : TabControl
    {
        private TabPage m_DraggedTab;

        public DraggableTabControl()
        {
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
        }

        private TabPage TabAt(Point position)
        {
            int nCount = TabCount;
            for (int i = 0; i < nCount; ++i)
            {
                if (GetTabRect(i).Contains(position))
                {
                    return TabPages[i];
                }
            }
            return null;
        }
        private void Swap(TabPage a, TabPage b)
        {
            int i = TabPages.IndexOf(a);
            int j = TabPages.IndexOf(b);
            TabPages[i] = b;
            TabPages[j] = a;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            m_DraggedTab = TabAt(e.Location);
        }
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            m_DraggedTab = null;
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (null == m_DraggedTab || e.Button != MouseButtons.Left)
            {
                return;
            }

            TabPage tab = TabAt(e.Location);
            if (null == tab || tab == m_DraggedTab)
            {
                return;
            }
            Swap(m_DraggedTab, tab);
            SelectedTab = m_DraggedTab;
        }
    }
}
