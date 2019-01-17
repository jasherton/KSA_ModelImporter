using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace KSA_ModelImporter
{
    public partial class Form1 : Form
    {
        Utility.BFRES model;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void LoadBFRES(string fname)
        {
            model = new Utility.BFRES();
            model.Evaluate(fname);

            if (model.Read())
            {
                LoadedName.Text = Path.GetFileNameWithoutExtension(fname);
                saveToolStripMenuItem.Visible = true;
                //MessageBox.Show("File Validated.", "BFRES File", MessageBoxButtons.OK);
            }
        }

        private void OpenFileStrip_Click(object sender, EventArgs e)
        {
            if (OpenFileDia.ShowDialog() == DialogResult.OK)
            {
                var fname = OpenFileDia.FileName;
                var nwext = Path.GetFileNameWithoutExtension(OpenFileDia.FileName);
                var ext = Path.GetExtension(OpenFileDia.FileName);
                var dir = Path.GetDirectoryName(OpenFileDia.FileName);
                if (ext == ".bfres")
                {
                    if (model != null) { model.Exit(); }
                    LoadBFRES(OpenFileDia.FileName);
                }
                else
                if (ext == ".cmp")
                {
                    bool allow = true;

                    if (model != null)
                    {
                        if (nwext == Path.GetFileName(model.FileName))
                        {
                            //oh boy this looks bad let's not do this
                            allow = false;
                            MessageBox.Show("File is currently loaded, cannot decompress.", "CMP File", MessageBoxButtons.OK);
                        }
                    }

                    if (allow)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = "cmd.exe";
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = "/C cd " + Directory.GetCurrentDirectory() + "/tools" + "&" + " quickbms -o KirbyStarAllies-Decompress.bms " + fname + " " + dir;


                        using (var Proc = Process.Start(startInfo))
                        {
                            Proc.WaitForExit();
                        }

                        var resp = MessageBox.Show("File decompressed. Want to load the file?", "CMP File", MessageBoxButtons.YesNo);

                        if (resp == DialogResult.Yes)
                        {
                            if (model != null) { model.Exit(); }
                            LoadBFRES(dir + "/" + nwext);
                        }
                    }

                }else
                {
                    MessageBox.Show("The file you selected is not .bfres or .cmp", "Invalid File Type", MessageBoxButtons.OK);
                }
            }
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            if (model != null)
            {
                if (model.FileOk)
                {
                    model.Exit();
                }
            }
            return;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDia.InitialDirectory = Path.GetDirectoryName(OpenFileDia.FileName);
            SaveFileDia.FileName = Path.GetFileName(OpenFileDia.FileName);
            if (SaveFileDia.ShowDialog() == DialogResult.OK)
            {
                if (SaveFileDia.FileName != null && SaveFileDia.FileName != "")
                {
                    //save file
                    if (File.Exists(SaveFileDia.FileName)) { File.Delete(SaveFileDia.FileName); };
                    bool IsSaved = model.SaveTo(SaveFileDia.FileName);
                    if (IsSaved)
                    {
                        return;
                    }   
                    
                }
            }
        }

        private void creditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
@"
Grand Dad - Tool, CMP Compressor

RandomTBush - CMP Decompressor

Luigi Auriemma - QuickBMS
", "Credits", MessageBoxButtons.OK);
        }
    }
}
