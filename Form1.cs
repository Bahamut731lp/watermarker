using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Text;

namespace Watermarker
{
    public partial class Form1 : Form
    {
        private Label inputLabel = new Label();
        private Label outputLabel = new Label();
        private Button runButton = new Button();
        private Button inputButton = new Button();
        private Button outputButton = new Button();
        private CheckBox outputSameAsInputCheckbox = new CheckBox();
        private ProgressBar progress = new ProgressBar();
        private string inputFolderPath;
        private string outputFolderPath;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            const int leftOffset = 10;
            const int spaceBetweenComponentBlocks = 25;

            //Nastavení okna formuláře
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.Size = new System.Drawing.Size(640, 360);
            this.Text = "Watermarker";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            //Nastavení labelu pro tlačítko pro výběr vstupní složky
            this.inputLabel.Text = "Vstupní cesta: ";
            this.inputLabel.Location = new System.Drawing.Point(leftOffset, 10);

            //Nastavení tlačítka pro výběr vstupní složky
            this.inputButton.Text = "Vybrat vstupní složku";
            this.inputButton.Location = new System.Drawing.Point(leftOffset, placeUnderComponent(inputLabel));
            this.inputButton.Size = new System.Drawing.Size(200, 25);
            this.inputButton.Click += (sender, e) => selectFolder(sender, e, false);

            //Nastavení labelu pro tlačítko pro výběr výstupní složky
            this.outputLabel.Text = "Výstupní cesta: ";
            this.outputLabel.Location = new System.Drawing.Point(leftOffset, placeUnderComponent(inputButton) + spaceBetweenComponentBlocks);

            //Nastavení tlačítka pro výběr výstupní složky
            this.outputButton.Text = "Vybrat výstupní složku";
            this.outputButton.Location = new System.Drawing.Point(leftOffset, placeUnderComponent(outputLabel));
            this.outputButton.Size = new System.Drawing.Size(200, 25);
            this.outputButton.Click += (sender, e) => selectFolder(sender, e, true);

            //Nastavení checkboxu pro zjištění, jestli je vstupní složka stejná jako výstupní
            this.outputSameAsInputCheckbox.Text = "Má být výstupní složka stejná jako vstupní? (Vytvoří se podsložka s výsledky)";
            this.outputSameAsInputCheckbox.AutoSize = true;
            this.outputSameAsInputCheckbox.Location = new System.Drawing.Point(leftOffset, placeUnderComponent(outputButton) + spaceBetweenComponentBlocks);

            //Nastavení tlačíka pro spuštění značkování obrázků
            this.runButton.Text = "Spustit";
            this.runButton.Location = new System.Drawing.Point(leftOffset, placeUnderComponent(outputSameAsInputCheckbox) + spaceBetweenComponentBlocks);
            this.runButton.Size = new System.Drawing.Size(200, 25);
            this.runButton.Click += new EventHandler(Main);

            this.progress.Location = new System.Drawing.Point(leftOffset, placeUnderComponent(runButton) + spaceBetweenComponentBlocks);
            this.progress.Size = new System.Drawing.Size(500, 25);

            Render(
                inputLabel, 
                inputButton, 
                outputLabel, 
                outputButton,
                outputSameAsInputCheckbox,
                runButton,
                progress
            );
        }
        private void selectFolder(object sender, EventArgs e, bool isItOutput) {
            using (var folderDialog = new FolderBrowserDialog()) //ANY dialog
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    if (isItOutput) outputFolderPath = folderDialog.SelectedPath;
                    else inputFolderPath = folderDialog.SelectedPath;                   
                }
            }
        }
        private int placeUnderComponent(Control component)
        {
            return component.Location.Y + component.Size.Height;
        }
        private void Render(params Control[] components)
        {
            foreach (Control component in components)
            {
                this.Controls.Add(component);
            }
        }

        private void Main(object sender, EventArgs e) {
            if (inputFolderPath == null || !Directory.Exists(inputFolderPath)) {
                MessageBox.Show("Nebyla vybrána žádná vstupní složka, nebo složka neexistuje!", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (outputFolderPath == null) {
                outputSameAsInputCheckbox.Checked = true;
            }

            if (this.outputSameAsInputCheckbox.Checked) {
                outputFolderPath = inputFolderPath + "\\výstup";
            }

            runButton.Enabled = false;

            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }


            DirectoryInfo di = new DirectoryInfo(inputFolderPath);
            FileInfo[] files = di.GetFiles("*.jpg");

            progress.Minimum = 1;
            progress.Maximum = files.Length;
            progress.Value = 1;
            progress.Step = 1;

            const float fontScaleFactor = 0.075f;

            foreach (var file in files)
            {
                Bitmap bitmap = null;               

                // Create from a stream so we don't keep a lock on the file.
                using (var stream = File.OpenRead(file.FullName))
                {
                    bitmap = (Bitmap)Bitmap.FromStream(stream);
                }

                using (bitmap)
                using (var graphics = Graphics.FromImage(bitmap))
                using (var font = new Font ("Monotype Corsiva", (float)(bitmap.Height * fontScaleFactor), FontStyle.Italic, GraphicsUnit.Pixel))
                {
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.DrawString("Angel Sarah", font, Brushes.Black, 0, bitmap.Height - 25 - (int)Math.Round((float)(bitmap.Height * fontScaleFactor)));
                    bitmap.Save(outputFolderPath + "/" + file.Name);
                
                    progress.PerformStep();
                }
            }

            runButton.Enabled = true;
            MessageBox.Show("Hotovo! Nově vytvořené fotky jsou uloženy v " + outputFolderPath, "Informace", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
