using CrazyLedsExport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CrazyLeds
{
    public partial class FormCrazyLeds : Form
    {
        public int GetCellSize()
        {
            return Math.Max(Math.Min(pictureBox.Width / GridSize.Width, pictureBox.Height / GridSize.Height), 10);
        }

        private CrazyDisplayType displayType;

        public CrazyDisplayType DisplayType
        {
            get { return displayType; }
            set
            {
                displayType = value;
                switch (displayType)
                {
                    case CrazyDisplayType.RGBMatrix:
                        groupBoxDotMatrix.Text = "Matrice RVB";
                        break;
                    case CrazyDisplayType.BWDisplay:
                        groupBoxDotMatrix.Text = "Image noir et blanc. Tout ce qui n'est pas noir est considéré comme blanc.";
                        break;
                    default:
                        groupBoxDotMatrix.Text = String.Empty;
                        break;
                }
            }
        }

        private string cheminFichier = null;

        public string CheminFichier
        {
            get { return cheminFichier;  }
            set
            {
                cheminFichier = value;
                if (Text.Contains("-"))
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        Text = Text.Substring(0, Text.IndexOf(" - "));
                    }
                    else
                    {
                        Text = Text.Substring(0, Text.IndexOf(" - ") + 3) + Path.GetFileNameWithoutExtension(cheminFichier);
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        Text = Text + " - " + Path.GetFileNameWithoutExtension(cheminFichier);
                    }
                }
            }
        }

        private Size gridSize;
        public Size GridSize {
            get { return gridSize; }
            set
            {
                gridSize = value;
                ledMatrix = new CrazyColor[GridSize.Width * GridSize.Height];
                for (int j = 0; j < GridSize.Height; ++j)
                {
                    for (int i = 0; i < GridSize.Width; ++i)
                    {
                        ledMatrix[i + j * GridSize.Width] = new CrazyColor();
                    }
                }
                DisplayMatrix();
            }
        }

        private CrazyColor[] ledMatrix = null;

        public FormCrazyLeds()
        {
            InitializeComponent();
            //pictureBox.Width = Math.Min(pictureBox.Size.Width, pictureBox.Size.Height);
            //pictureBox.Height = Math.Min(pictureBox.Size.Width, pictureBox.Size.Height);
            DisplayType = CrazyDisplayType.RGBMatrix;
            PopulatePaletteBoxes();
            GridSize = new Size(10, 10);
            PopulateComPortList();
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void nouveauToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormNew form = new FormNew();
            form.GridWidth = GridSize.Width;
            form.GridHeight = GridSize.Height;
            form.DisplayType = DisplayType;
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                // Create a new picture with the given dimensions
                DisplayType = form.DisplayType;
                GridSize = new Size(form.GridWidth, form.GridHeight);
                CheminFichier = null;
            }
            
        }

        private void PopulatePaletteBoxes()
        {
            panelPalette.SuspendLayout();

            for (int i = 0; i < 16; ++i)
            {
                var component = new PictureBox();
                component.Width = 40;
                component.Height = 40;
                panelPalette.Controls.Add(component);
                component.BackColor = Color.White;
                component.Click += MouseEventHandlerPaletteButtonClick;
                component.DoubleClick += MouseEventHandlerPaletteButtonDoubleClick;
            }

            panelPalette.ResumeLayout();
        }

        private void MouseEventHandlerPaletteButtonClick(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                buttonForeColor.BackColor = ((Control)sender).BackColor;
            }
            else if (((MouseEventArgs)e).Button == MouseButtons.Right)
            {
                buttonBackColor.BackColor = ((Control)sender).BackColor;
            }
        }

        private void MouseEventHandlerPaletteButtonDoubleClick(object sender, EventArgs e)
        {
            colorDialog.Color = ((Control)sender).BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                ((Control)sender).BackColor = colorDialog.Color;
            }
        }

        private void enregistrerSousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.RestoreDirectory = true;
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = ".led";
            dlg.Filter = "LED (*.led)|*.led";
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                CrazyLeds.Global data = BuildSaveData();
                XmlSerializer serializer = new XmlSerializer(typeof(CrazyLeds.Global));
                TextWriter writer = new StreamWriter(dlg.OpenFile());
                serializer.Serialize(writer, data);
                writer.Close();
                CheminFichier = dlg.FileName;
            }
        }

        private void enregistrerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(CheminFichier))
            {
                enregistrerSousToolStripMenuItem_Click(sender, e);
            }
            else
            {
                CrazyLeds.Global data = BuildSaveData();
                XmlSerializer serializer = new XmlSerializer(typeof(CrazyLeds.Global));
                TextWriter writer = new StreamWriter(CheminFichier);
                serializer.Serialize(writer, data);
                writer.Close();
            }
        }

        private CrazyLeds.Global BuildSaveData()
        {
            CrazyLeds.Global data = new CrazyLeds.Global();

            data.DisplayType = DisplayType;

            data.Width = GridSize.Width;
            data.Height = GridSize.Height;

            data.ForeGroundColor = new CrazyColor(buttonForeColor.BackColor);
            data.BackGroundColor = new CrazyColor(buttonBackColor.BackColor);

            foreach (Control ctrl in panelPalette.Controls)
            {
                data.CustomColorsList.Add(new CrazyColor(ctrl.BackColor));
            }

            data.LedMatrix = new List<CrazyColor>(GridSize.Width * GridSize.Height);
            data.LedMatrix.AddRange(ledMatrix);

            return data;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.AddExtension = true;
            dlg.CheckFileExists = true;
            dlg.DefaultExt = ".led";
            dlg.Filter = "LED (*.led)|*.led";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Global));
                using (Stream stream = dlg.OpenFile())
                {
                    Global data = (Global)serializer.Deserialize(stream);
                    DispatchDataToControls(data);
                }
                CheminFichier = dlg.FileName;
            }
        }

        private void DispatchDataToControls(Global data)
        {
            DisplayType = data.DisplayType;
            GridSize = new Size(data.Width, data.Height);
            buttonForeColor.BackColor = data.ForeGroundColor.ToColor();
            buttonBackColor.BackColor = data.BackGroundColor.ToColor();

            int i = 0;
            foreach (Control ctrl in panelPalette.Controls)
            {
                ctrl.BackColor = data.CustomColorsList[i++].ToColor();
            }
            data.LedMatrix.CopyTo(ledMatrix);
            DisplayMatrix();
        }

        private void DisplayMatrix()
        {
            vScrollBar.Minimum = 0;
            vScrollBar.Maximum = GridSize.Height - 1;
            vScrollBar.LargeChange = pictureBox.Height / GetCellSize();
            vScrollBar.Value = 0;
            hScrollBar.Minimum = 0;
            hScrollBar.Maximum = GridSize.Width - 1;
            hScrollBar.LargeChange = pictureBox.Width / GetCellSize();
            hScrollBar.Value = 0;
            pictureBox.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.O))
            {
                openToolStripMenuItem_Click(this, new EventArgs());
                return true;
            }
            if (keyData == (Keys.Control | Keys.S))
            {
                enregistrerToolStripMenuItem_Click(this, new EventArgs());
                return true;
            }
            if (keyData == (Keys.Control | Keys.N))
            {
                nouveauToolStripMenuItem_Click(this, new EventArgs());
                return true;
            }
            if (keyData == (Keys.Control | Keys.C))
            {
                copierToolStripMenuItem1_Click(this, new EventArgs());
                return true;
            }
            if (keyData == (Keys.Control | Keys.V))
            {
                collerToolStripMenuItem_Click(this, new EventArgs());
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PopulateComPortList()
        {
            foreach (string ComPort in SerialPort.GetPortNames())
            {
                comboBoxComPortList.Items.Clear();
                comboBoxComPortList.Items.Add(ComPort);
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            SerialPort port = new SerialPort(comboBoxComPortList.Text, 57600);
            port.Open();
            port.Write("F");
            byte[] couleurBytes = new byte[3];
            foreach (CrazyColor couleur in ledMatrix)
            {
                couleurBytes[0] = couleur.R;
                couleurBytes[1] = couleur.G;
                couleurBytes[2] = couleur.B;
                port.Write(couleurBytes, 0, 3);
            }
            port.Close();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SerialPort port = new SerialPort(comboBoxComPortList.Text, 57600);
            port.Open();
            port.Write("L");
            byte[] light = new byte[1];
            light[0] = (byte)trackBar1.Value;
            port.Write(light, 0, 1);
            port.Close();
        }

        private void comboBoxComPortList_DropDown(object sender, EventArgs e)
        {
            PopulateComPortList();
        }

        private void exporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CheminFichier))
            {
                string outputFileContent;
                if (DisplayType == CrazyDisplayType.RGBMatrix)
                {
                    outputFileContent = ExportManager.ExportLedImageToHeaderFile(ledMatrix, GridSize.Width, GridSize.Height, new CrazyColor(buttonBackColor.BackColor), Path.GetFileNameWithoutExtension(cheminFichier));
                }
                else
                {
                    outputFileContent = ExportManager.ExportBWImageToHeaderFile(ledMatrix, GridSize.Width, GridSize.Height, new CrazyColor(buttonBackColor.BackColor), Path.GetFileNameWithoutExtension(cheminFichier));
                }
                File.WriteAllText(
                    Path.Combine(
                        Path.GetDirectoryName(CheminFichier),
                        Path.GetFileNameWithoutExtension(CheminFichier) + ".h"),
                    outputFileContent);
            }
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            CrazyColor color = null;
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                color = new CrazyColor(buttonForeColor.BackColor);
            }
            else if (((MouseEventArgs)e).Button == MouseButtons.Right)
            {
                color = new CrazyColor(buttonBackColor.BackColor);
            }
            if (color != null)
            {
                // Determine the target cell
                int cellSize = GetCellSize();
                int i = e.X / cellSize;
                int j = e.Y / cellSize;
                if (i + hScrollBar.Value < GridSize.Width && j + vScrollBar.Value < GridSize.Height)
                {
                    ledMatrix[i + hScrollBar.Value + (j + vScrollBar.Value) * GridSize.Width] = color;
                    pictureBox.Invalidate(new Rectangle(i * cellSize, j * cellSize, cellSize, cellSize));
                }
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            // Determine the cells to redraw
            int cellSize = GetCellSize();
            int iMin = e.ClipRectangle.Left / cellSize;
            int iMax = Math.Min(e.ClipRectangle.Right / cellSize, GridSize.Width - 1);
            int jMin = e.ClipRectangle.Top / cellSize;
            int jMax = Math.Min(e.ClipRectangle.Bottom / cellSize, GridSize.Height - 1);

            // Clear the whole area
            e.Graphics.FillRectangle(Brushes.LightGray, e.ClipRectangle);

            // Draw cells
            for (int i = iMin; i <= iMax; ++i)
            {
                if (i + hScrollBar.Value < GridSize.Width)
                {
                    for (int j = jMin; j <= jMax; ++j)
                    {
                        if (j + vScrollBar.Value < GridSize.Height)
                        {
                            e.Graphics.FillRectangle(new SolidBrush(ledMatrix[(i + hScrollBar.Value) + (j + vScrollBar.Value) * GridSize.Width].ToColor()), i * cellSize, j * cellSize, cellSize, cellSize);
                        }
                    }
                }
            }
            // Draw grid
            for (int i = iMin; i <= iMax; ++i)
            {
                for (int j = jMin; j <= jMax; ++j)
                {
                    e.Graphics.DrawRectangle(Pens.DarkGray, i * cellSize, j * cellSize, cellSize, cellSize);
                }                
            }
        }

        private void copierToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(GridSize.Width, GridSize.Height);
            for (int i = 0; i < GridSize.Width; ++i)
            {
                for (int j = 0; j < GridSize.Height; ++j)
                {
                    img.SetPixel(i, j, ledMatrix[i + j * GridSize.Width].ToColor());
                }
            }
            Clipboard.SetImage(img);
        }

        private void collerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap image = null;
            if (Clipboard.ContainsImage())
            {
                image = Clipboard.GetImage() as Bitmap;
            }
            if (image != null)
            {
                for (int y = 0; y < GridSize.Height && y < image.Height; ++y)
                {
                    for (int x = 0; x < GridSize.Width && x < image.Width; ++x)
                    {
                        ledMatrix[x + y * GridSize.Width] = new CrazyColor(image.GetPixel(x, y));
                    }
                }
                DisplayMatrix();
            }
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
            System.Diagnostics.Debug.WriteLine(vScrollBar.Minimum + " / " + vScrollBar.Value + " / " + vScrollBar.Maximum);
        }

        private void hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }
    }
}
