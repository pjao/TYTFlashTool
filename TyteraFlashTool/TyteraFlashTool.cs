using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;
using dfulib;
using System.Windows.Threading;
using System.Deployment;

namespace TyteraFlashTool
{
    public partial class TyteraFlashToolForm : Form
    {
        private WebClient webc = new WebClient();
        private TyteraRadio tr = null;
        private bool downloading = false;

        public TyteraFlashToolForm()
        {
            InitializeComponent();

            webc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
            webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);

            //tr.TickleTickle();
            cbVersao.SelectedIndex = 0;
            
            Timer.Start();
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            nsProgressBar.Value = e.ProgressPercentage;
        }

        private string DLFileName = "";
        private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (DLFileName == "userdb.bin")
                StatusLabel.Text = "Number of Users: " + (File.ReadLines("userdb.bin").Count() - 1);
            else
                StatusLabel.Text = "Done Downloading. Press Flash Button.";
            downloading = false;
            this.nsProgressBar.Invalidate();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog OF = new OpenFileDialog();
            
            OF.Filter = "Firmware or UserDB (*.bin)|*.*|Codeplug Files | (*.dfu)|All files (*.*)|*.*";
            DialogResult r = OF.ShowDialog();
            if(r == DialogResult.OK)
            {
                if(File.Exists(OF.FileName))
                {
                    //StatusLabel.Text = "opened file...";
                    FilenameTextBox.Text = OF.FileName;
                }
            }
        }

        // codeplug
        private void WriteButton_Click(object sender, EventArgs e)
        {
            if (FilenameTextBox.Text.Equals(""))
            {
                ThemeMessageBox.ShowMessageBox("Please open a codeplug!");
                return;
            }
            byte[] cp = File.ReadAllBytes(FilenameTextBox.Text);
            byte[] header = new byte[4];
            Array.Copy(cp, 0, header, 0, 4);
            UInt32 inn = BitConverter.ToUInt32(header, 0);
            
            if (inn != 0x53756644)
            {
                StatusLabel.Text = "writing codeplug...";
                (new Thread(() => {
                    tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                    Thread.CurrentThread.IsBackground = true;
                    tr.WriteCodeplug(FilenameTextBox.Text);
                    tr.Reboot();
                    //this.StatusLabel.Text = "Done";
                    //this.nsProgressBar.Value = 0;
                })).Start();
            }
            else
            {
                ThemeMessageBox.ShowMessageBox("Invalid file type.  Must be .dfu");
                return;
            }
        }

        private void ReadButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog textDialog = new SaveFileDialog();
            textDialog.Filter = "Codeplug Files | *.dfu";
            textDialog.DefaultExt = "dfu";

            DialogResult r = textDialog.ShowDialog();
            if (r == DialogResult.OK)
            {
                StatusLabel.Text = "reading codeplug...";
                (new Thread(() => {
                    Thread.CurrentThread.IsBackground = true;
                    tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                    tr.ReadClodeplug(textDialog.FileName);
                    tr.Reboot();
                    //this.StatusLabel.Text = "Done";
                    //this.nsProgressBar.Value = 0;
                })).Start();
            }
        }

        // firmware
        private void DownloadGPSButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = cbVersao.SelectedItem + ". Downloading gps_fw.bin ...";
            downloading = true;

            switch (cbVersao.SelectedItem)
            {
                case "Ecrã Novo - Letra Preta":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AABD2GHgJkq-D2iNliIY9s-La/firmware-GPS.bin?dl=1"), "gps_fw.bin");
                    break;
                case "Ecrã Novo - Letra Branca":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AADEGwt_qjgiPc3B6YyjryKFa/firmware-GPS-White.bin?dl=1"), "gps_fw.bin");
                    break;
                case "Ecrã Antigo":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AADqbd7Figg9xG0-YY7mfg9Xa/firmware-GPS-Legacy.bin?dl=1"), "gps_fw.bin");
                    break;
                case "MD380Tools Travis":
                    webc.DownloadFileAsync(new Uri("https://pd0zry.nl/md380-fw/00_latest_firmware-S13.020.bin"), "gps_fw.bin");
                    break;
                case "Firmware Original":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AABQ8MWFRxzXcxoenH3dRlC7a/S013.020.bin?dl=1"), "gps_fw.bin");
                    break;
                default:
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AABD2GHgJkq-D2iNliIY9s-La/firmware-GPS.bin?dl=1"), "gps_fw.bin");
                    break;

            }


//            webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AABD2GHgJkq-D2iNliIY9s-La/firmware-GPS.bin?dl=1"), "gps_fw.bin");

            if(File.Exists("nongps_fw.bin"))
            {
                File.Delete("nongps_fw.bin");
            }

        }

        private void DownloadNONGPSButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = cbVersao.SelectedItem + ". Downloading nongps_fw.bin ...";
            DLFileName = "fw.bin";
            downloading = true;


            switch (cbVersao.SelectedItem) {
                case "Ecrã Novo - Letra Preta":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AACGA-MHWHgstYODs3muypxMa/firmware-noGPS.bin?dl=1"), "nongps_fw.bin");
                    break;
                case "Ecrã Novo - Letra Branca":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AABwZqjWMay-rKeHR_Qpo6SAa/firmware-noGPS-White.bin?dl=1"), "nongps_fw.bin");
                    break;
                case "Ecrã Antigo":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AAD6gj3kxXfMuEMkPIcmO_IBa/firmware-noGPS-Legacy.bin?dl=1"), "nongps_fw.bin");
                    break;
                case "MD380Tools Travis":
                    webc.DownloadFileAsync(new Uri("https://pd0zry.nl/md380-fw/00_latest_firmware.bin"), "nongps_fw.bin");
                    break;
                case "Firmware Original":
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AABqr94xAXF4pHIUwLR3u7aQa/D013.020.bin?dl=1"), "nongps_fw.bin");
                    break;
                default:
                    webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AACGA-MHWHgstYODs3muypxMa/firmware-noGPS.bin?dl=1"), "nongps_fw.bin");
                    break;

            }

//            webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AACGA-MHWHgstYODs3muypxMa/firmware-noGPS.bin?dl=1"), "nongps_fw.bin");

            if (File.Exists("gps_fw.bin"))
            {
                File.Delete("gps_fw.bin");
            }
        }

       

        private void FlashFirmwareButton_Click(object sender, EventArgs e)
        {
            if (File.Exists("gps_fw.bin") && File.Exists("nongps_fw.bin"))
            {
                ThemeMessageBox.ShowMessageBox("Please download only one firmware!");
            }
            else
            {
                if(File.Exists("gps_fw.bin"))
                {
                    StatusLabel.Text = "writing gps_fw.bin ...";
                    (new Thread(() => {
                        tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                        Thread.CurrentThread.IsBackground = true;
                        tr.WriteFirmware("gps_fw.bin");
                        tr.Reboot();
                    })).Start();
                }
                else if(File.Exists("nongps_fw.bin"))
                {
                    StatusLabel.Text = "writing nongps_fw.bin ...";
                    (new Thread(() => {
                        tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                        Thread.CurrentThread.IsBackground = true;
                        tr.WriteFirmware("nongps_fw.bin");
                        tr.Reboot();
                    })).Start();
                }
            }
        }

        private void FlashFirmwareFileButton_Click(object sender, EventArgs e)
        {
            if (!FilenameTextBox.Text.Equals(""))
            {
                StatusLabel.Text = "writing firmware ...";
                (new Thread(() => {
                    tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                    Thread.CurrentThread.IsBackground = true;
                    tr.WriteFirmware(FilenameTextBox.Text);
                    tr.Reboot();
                })).Start(); 
            }
            else
            {
                ThemeMessageBox.ShowMessageBox("Please open a firmware!");
            }
        }
        
        private void flashUserDB()
        {
            tr.TickleTickle();
            tr.GetSpiID();
            tr.WriteUserDB("userdb.bin");
            tr.Reboot();
            this.StatusLabel.Text = "Done";
            this.nsProgressBar.Value = 0;
        }

        private void flashUserDBFile()
        {
            tr.TickleTickle();
            tr.GetSpiID();
            tr.WriteUserDB(FilenameTextBox.Text);
            tr.Reboot();
            this.StatusLabel.Text = "Done";
            this.nsProgressBar.Value = 0;
        }



        void bw_DoWorkWriteUserDBFile(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = sender as BackgroundWorker;

            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                flashUserDBFile();
            });
        }

        void bw_DoWorkWriteUserDB(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = sender as BackgroundWorker;
            
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                flashUserDB();
            });


        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            nsProgressBar.Value = e.ProgressPercentage;
        }

        BackgroundWorker bw;

        // userdb
        private void DownloadUserDBButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "downloading userdb.bin ...";
            /*nsProgressBar.Minimum = 0;
            nsProgressBar.Maximum = 100;
            bw = new BackgroundWorker();

            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            nsProgressBar.Value = 0;
            
            bw.RunWorkerAsync();*/
            DLFileName = "userdb.bin";
            downloading = true;
            webc.DownloadFileAsync(new Uri("https://www.dropbox.com/sh/utb53v54wph7q8x/AAB9TpX1gAft3_m17I3eY144a/userdb.bin?dl=1"), "userdb.bin");



        }

        private void FlashUserDBButton_Click(object sender, EventArgs e)
        {
            if(File.Exists("userdb.bin"))
            {
                StatusLabel.Text = "writing userdb.bin ...";
                tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                /*tr.WriteUserDB("userdb.bin");
                tr.Reboot();*/

                nsProgressBar.Minimum = 0;
                nsProgressBar.Maximum = 100;
                bw = new BackgroundWorker();

                bw.WorkerSupportsCancellation = true;
                bw.WorkerReportsProgress = true;
                bw.DoWork += new DoWorkEventHandler(bw_DoWorkWriteUserDB);
                bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                nsProgressBar.Value = 0;
                
                bw.RunWorkerAsync();

                /*(new Thread(() => {
                    Thread.CurrentThread.IsBackground = true;
                    //tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                    tr.WriteUserDB("userdb.bin");
                    tr.Reboot();
                })).Start();*/
            }
            else
            {
                ThemeMessageBox.ShowMessageBox("Please download a userdb!");
            }
        }

        private void FlashfromFileUserDBButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "writing userDB ...";
            tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);

            nsProgressBar.Minimum = 0;
            nsProgressBar.Maximum = 100;
            bw = new BackgroundWorker();

            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWorkWriteUserDBFile);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            nsProgressBar.Value = 0;

            bw.RunWorkerAsync();

            /*(new Thread(() => {
                tr = new TyteraRadio(TyteraRadio.RadioModel.RM_MD380);
                tr.WriteUserDB(FilenameTextBox.Text);
                tr.Reboot();
            })).Start();*/
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(tr != null && !downloading)
            {
                int newInt = (int)tr.GetCurrentOperationProgress();
                if(newInt != nsProgressBar.Value)
                {
                    if(newInt == 0)
                    {
                        this.StatusLabel.Text = "Done";
                    }
                    nsProgressBar.Value = newInt;
                    nsProgressBar.Refresh();
                }
            }
        }

        private void nsProgressBar_Click(object sender, EventArgs e)
        {

        }

        private void nsTheme_Click(object sender, EventArgs e)
        {

        }

        private void FilenameLabel_Click(object sender, EventArgs e)
        {

        }

        private void nsLabel1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
