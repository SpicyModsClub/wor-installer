using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows.Forms;

using Microsoft.Win32;

namespace WoR_Installer
{
    public partial class Form1 : Form
    {
        private string installLocation;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.installLocation =
                    (string)
                    Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Aspyr\Guitar Hero III", "Path", null);

                if (string.IsNullOrEmpty(this.installLocation))
                {
                    // Not likely but people might still be running 32-bit; try that
                    this.installLocation =
                        (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Aspyr\Guitar Hero III", "Path", null);
                }
            }
            catch (SecurityException)
            {
                MessageBox.Show(
                    "You don't have permissions to read from the registry!  Please make sure you are running as administrator.");
            }
            catch
            {
                // Can't find the install location; the user can locate it themself
            }

            if (!string.IsNullOrEmpty(this.installLocation))
            {
                this.txtPath.Text = this.installLocation;
                this.ofd.InitialDirectory = this.installLocation;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK != ofd.ShowDialog())
            {
                return;
            }

            this.installLocation = Path.GetDirectoryName(this.ofd.FileName);
            this.txtPath.Text = this.installLocation;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            ((Control)sender).Enabled = false;

            var di = new DirectoryInfo(this.installLocation);
            int failedFiles = 0;

            try
            {
                foreach (var f in di.GetFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        f.Attributes &= ~FileAttributes.ReadOnly;
                    }
                    catch
                    {
                        failedFiles++;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Installation directory not found.");
                ((Control)sender).Enabled = true;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ((Control) sender).Enabled = true;
                return;
            }

            if (failedFiles > 0) { 
                var dr = MessageBox.Show(
                                "Did not have permissions to set some files to writable.  The mod may not install correctly.  Please make sure you are running as administrator.  Try to continue anyway?",
                                "Permissions Error",
                                MessageBoxButtons.YesNo);

                if (dr == DialogResult.No) {
                    ((Control) sender).Enabled = true;
                    return;
                }
            }

            var arguments = string.Format("x -o\"{0}\" -y Data.7z", this.installLocation);
            ProcessStartInfo info = new ProcessStartInfo("7za.exe", arguments);
            try
            {
               var extractor = Process.Start(info);

                Cursor = Cursors.WaitCursor;
                extractor.WaitForExit();
                Cursor = Cursors.Default;

            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                ((Control) sender).Enabled = true;
                return;
            }

            MessageBox.Show("Installation complete!");
            ((Control) sender).Enabled = true;
        }
    }
}
