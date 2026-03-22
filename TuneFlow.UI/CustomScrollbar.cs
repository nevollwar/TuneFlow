using System;
using System.Drawing;
using System.Windows.Forms;

namespace TuneFlow.UI
{
    public class CustomScrollbar : Control
    {
        public DataGridView Grid { get; set; }
        private int _thumbY = 0;
        private int _thumbHeight = 40;
        private bool _isDragging = false;
        private int _dragOffset = 0;
        private bool _isHovering = false;

        public CustomScrollbar()
        {
            this.DoubleBuffered = true;
            this.Width = 12;
            this.Cursor = Cursors.Default;
        }

        public void UpdateScroll()
        {
            if (Grid == null || Grid.RowCount == 0) return;

            int visibleRows = Grid.DisplayedRowCount(false);
            if (visibleRows >= Grid.RowCount)
            {
                this.Visible = false;
                return;
            }

            this.Visible = true;
            _thumbHeight = Math.Max(30, (int)((float)visibleRows / Grid.RowCount * this.Height));

            int maxScroll = Grid.RowCount - visibleRows;
            if (maxScroll <= 0) maxScroll = 1;

            _thumbY = (int)(((float)Grid.FirstDisplayedScrollingRowIndex / maxScroll) * (this.Height - _thumbHeight));
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(18, 18, 18));
            if (!this.Visible) return;

            Color c = _isDragging ? Color.White : (_isHovering ? Color.FromArgb(179, 179, 179) : Color.FromArgb(83, 83, 83));
            e.Graphics.FillRectangle(new SolidBrush(c), 2, _thumbY, this.Width - 4, _thumbHeight);
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovering = true; this.Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { _isHovering = false; this.Invalidate(); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Y >= _thumbY && e.Y <= _thumbY + _thumbHeight)
                {
                    _isDragging = true;
                    _dragOffset = e.Y - _thumbY;
                }
                else if (Grid != null)
                {
                    // Прыжок к месту клика
                    float ratio = (float)e.Y / this.Height;
                    int visible = Grid.DisplayedRowCount(false);
                    int targetRow = (int)(ratio * (Grid.RowCount - visible));
                    Grid.FirstDisplayedScrollingRowIndex = Math.Max(0, Math.Min(targetRow, Grid.RowCount - 1));
                    UpdateScroll();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) { _isDragging = false; this.Invalidate(); }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging && Grid != null)
            {
                int maxY = this.Height - _thumbHeight;
                int newY = Math.Max(0, Math.Min(e.Y - _dragOffset, maxY));
                int visible = Grid.DisplayedRowCount(false);
                int targetRow = (int)((float)newY / maxY * (Grid.RowCount - visible));

                Grid.FirstDisplayedScrollingRowIndex = Math.Max(0, Math.Min(targetRow, Grid.RowCount - 1));
                UpdateScroll();
            }
        }
    }
}