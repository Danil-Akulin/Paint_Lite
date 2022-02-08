using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Lab2
{

    public partial class Form1 : Form
    {

        bool drawing;
        int historyCounter; // Счетчик истории

        GraphicsPath currentPath;
        Point oldLocation;
        Pen currentPen;
        Color historyColor; //Сохранение текущего цвета перед использованием ластика
        List<Image> History; // Список для истории

        public Form1()
        {
            InitializeComponent();
            drawing = false;                     //Переменная, ответственная за рисование
            currentPen = new Pen(Color.Black);   //Инициализация пера с черным цветом
            currentPen.Width = trackBarPen.Value; //Инициализация толщины пера
            History = new List<Image>();            //Инициализация списка для истории
        }

        private bool CheckSave(object sender, EventArgs e)
        {
            if (picDrawingSurface.Image != null)
            {

                var result = MessageBox.Show("Сохранить текущее изображение?", "Предупреждение", MessageBoxButtons.YesNoCancel);

                switch (result)
                {
                    case DialogResult.No: break;
                    case DialogResult.Yes: SaveFile(sender, e); break;
                    case DialogResult.Cancel: return true;
                }
            }
            return false;
        }

        private void NewFile(object sender, EventArgs e)
        {
            if (History.Count != 0 && historyCounter != 0)
            {
                if (CheckSave(sender, e)) return;
            }

            History.Clear();
            historyCounter = 0;
            Bitmap pic = new Bitmap(750, 500);
            picDrawingSurface.Image = pic;
            WhiteBack();
            History.Add(new Bitmap(picDrawingSurface.Image));
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile(sender, e);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            NewFile(sender, e);
        }

        private void WhiteBack()
        {
            Graphics g = Graphics.FromImage(picDrawingSurface.Image);
            g.Clear(Color.White);
            g.DrawImage(picDrawingSurface.Image, 0, 0, 750, 500);
        }
      

        private void SaveFile(object sender, EventArgs e)
        {
            SaveFileDialog SaveDlg = new SaveFileDialog();
            SaveDlg.Filter = "JPEG Image|*.jpg|BitmapImage|*.bmp|GIFImage|*.gif|PNGImage|*.png";
            SaveDlg.Title = "Save an Image File";
            SaveDlg.FilterIndex = 4;    //По умолчанию будет выбрано последнее расширение *.png

            SaveDlg.ShowDialog();

            if (SaveDlg.FileName != "")     //Если введено не пустое имя
            {
                System.IO.FileStream fs = (System.IO.FileStream)SaveDlg.OpenFile();

                switch (SaveDlg.FilterIndex)
                {
                    case 1:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Jpeg);

                        break;

                    case 2:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Bmp);
                        break;

                    case 3:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Gif);
                        break;

                    case 4:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Png);
                        break;
                }

                fs.Close();
            }
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(sender, e);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            SaveFile(sender, e);
        }

        private void OpenFile(object sender, EventArgs e)
        {
            if (History.Count != 0 && historyCounter != 0)
            {
                if (CheckSave(sender, e)) return;
            }

            OpenFileDialog OP = new OpenFileDialog();
            OP.Filter = "JPEG Image|*.jpg|BitmapImage|*.bmp|GIFImage|*.gif|PNGImage|*.png";
            OP.Title = "Open an Image File";
            OP.FilterIndex = 1; //По умолчанию будет выбрано первое расширение *.jpg


            // когда пользователь укажет нужный путь к картинке, ее нужно будет загрузить в PictureBox:
            if (OP.ShowDialog() != DialogResult.Cancel)
            {
                picDrawingSurface.Load(OP.FileName);

                History.Clear();
                historyCounter = 0;
                History.Add(new Bitmap(picDrawingSurface.Image));
            }


            picDrawingSurface.AutoSize = true;


        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile(sender, e);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenFile(sender, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void picDrawingSurface_MouseDown(object sender, MouseEventArgs e)
        {
            if (picDrawingSurface.Image == null)
            {
                MessageBox.Show("Сначала создайте новый файл!");
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                historyColor = currentPen.Color;
                drawing = true;
                oldLocation = e.Location;
                currentPath = new GraphicsPath();
            }

            if (e.Button == MouseButtons.Right)
            {
                historyColor = currentPen.Color; // сохранить текущий цвет пера
                currentPen.Color = Color.White;
                drawing = true;
                oldLocation = e.Location;
                currentPath = new GraphicsPath();
            }

        }

        private void picDrawingSurface_MouseUp(object sender, MouseEventArgs e)
        {

            if (drawing)
            {


                drawing = false;
                currentPen.Color = historyColor;

                //Очистка ненужной истории
                History.RemoveRange(historyCounter + 1, History.Count - historyCounter - 1);
                History.Add(new Bitmap(picDrawingSurface.Image));
                if (historyCounter + 1 < 10) historyCounter++;
                if (History.Count - 1 == 10) History.RemoveAt(0);
                try
                {
                    currentPath.Dispose();
                }
                catch { };
            }


        }

        private void picDrawingSurface_MouseMove(object sender, MouseEventArgs e)
        {
            label_XY.Text = e.X.ToString() + ", " + e.Y.ToString();
            if (drawing)
            {
                try
                {
                    Graphics g = Graphics.FromImage(picDrawingSurface.Image);
                    currentPath.AddLine(oldLocation, e.Location);
                    g.DrawPath(currentPen, currentPath);
                    oldLocation = e.Location;
                    g.Dispose();
                    picDrawingSurface.Invalidate();
                }
                catch { };
            }

        }

        private void trackBarPen_Scroll(object sender, EventArgs e)
        {
            currentPen.Width = trackBarPen.Value;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (History.Count != 0 && historyCounter != 0)
            {
                picDrawingSurface.Image = new Bitmap(History[--historyCounter]);
            }
            else MessageBox.Show("История пуста");

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (historyCounter < History.Count - 1)
            {
                picDrawingSurface.Image = new Bitmap(History[++historyCounter]);
            }
            else MessageBox.Show("История пуста");

        }

        private void solidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Solid;

            solidToolStripMenuItem.Checked = true;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = false;

        }

        private void dotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Dot;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = true;
            dashDotDotToolStripMenuItem.Checked = false;
        }

        private void dashDotDotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.DashDotDot;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = true;
        }

        private void SetColor(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentPen.Color = colorDialog.Color;
            }

        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetColor(sender, e);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            SetColor(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout AddRec = new FormAbout();

            AddRec.Owner = this;
            AddRec.ShowDialog();
        }

        private void picDrawingSurface_MouseLeave(object sender, EventArgs e)
        {
            label_XY.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (History.Count != 0 && historyCounter != 0)
            {
                if (CheckSave(sender, e)) return;
            }
        }
    }
}
