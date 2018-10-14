using CrazyLedsExport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrazyLeds
{
    public partial class FormNew : Form
    {
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public CrazyDisplayType DisplayType { get; set; }

        public FormNew()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (ValidateDimensions())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateDimensions()
        {
            bool result = false;

            if (radioButtonRGBMatrix.Checked)
            {
                DisplayType = CrazyDisplayType.RGBMatrix;
            }
            else if (radioButtonBWScreen.Checked)
            {
                DisplayType = CrazyDisplayType.BWDisplay;
            }

            int num;
            if (int.TryParse(textBoxWidth.Text, out num))
            {
                GridWidth = num;
                if (int.TryParse(textBoxHeight.Text, out num))
                {
                    GridHeight = num;
                    result = true;
                }
                else
                {
                    textBoxHeight.Select();
                }
            }
            else
            {
                textBoxWidth.Select();
            }
            return result;
        }

        private void FormNew_Load(object sender, EventArgs e)
        {
            textBoxWidth.Text = GridWidth.ToString();
            textBoxHeight.Text = GridHeight.ToString();
            switch (DisplayType)
            {
                case CrazyDisplayType.RGBMatrix:
                    radioButtonRGBMatrix.Select();
                    break;
                case CrazyDisplayType.BWDisplay:
                    radioButtonBWScreen.Select();
                    break;
            }
        }
    }
}
