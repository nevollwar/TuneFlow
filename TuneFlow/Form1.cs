using System;
using TuneFlow.Core;

namespace TuneFlow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Метод срабатывает, когда курсор мыши заходит на область кнопки.
        /// </summary>
        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            // Приводим "sender" к типу Button, чтобы работать с ним
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.BackColor = Color.FromArgb(40, 40, 40); // Меняем фон на темно-серый
                btn.ForeColor = Color.FromArgb(30, 215, 96); // Текст становится зеленым (Spotify)
            }
        }

        /// <summary>
        /// Метод срабатывает, когда курсор мыши покидает область кнопки.
        /// </summary>
        private void MenuButton_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.BackColor = Color.Black; // Возвращаем черный фон
                btn.ForeColor = Color.Gray;  // Возвращаем серый текст
            }
        }


        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnMenuCatalog_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabPage1;
        }

        private void btnMenuPlaylists_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabPage2;
        }

        private void btnMenuRecs_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabPage3;
        }

        private void btnMenuStats_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabPage4;
        }
    }
}
