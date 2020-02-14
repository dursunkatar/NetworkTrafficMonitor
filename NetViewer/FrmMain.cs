using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetViewer
{
    public partial class FrmMain : Form
    {
        private readonly List<Connection> baglantilar;
        private readonly Process cmd;
        private int imageIndex = 1;
        public FrmMain()
        {
            InitializeComponent();
            baglantilar = new List<Connection>();
            cmd = new Process();
            init();
        }

        private void init()
        {
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
        }
        private void listele()
        {
            string line = null;
            cmd.Start();
            cmd.StandardInput.WriteLine("netstat -ano | findstr ESTABLISHED");
            cmd.StandardInput.WriteLine("netstat -ano | findstr SYN_SENT");
            cmd.StandardInput.WriteLine("exit");
            while ((line = cmd.StandardOutput.ReadLine()) != null)
            {
                if ((line.Contains("ESTABLISHED") || line.Contains("SYN_SENT")) && !line.Contains("findstr"))
                {
                    var con = ayikla(line.Split(' '));

                    if (con.HedefIp.Split(':')[0] == "127.0.0.1" || con.KaynakIp.Split(':')[0] == "127.0.0.1")
                    {
                        continue;
                    }
                    if (!listedeVarmi(con))
                    {
                        var lvi = new ListViewItem();
                        lvi.Text = con.Program;

                        lvi.SubItems.Add(con.KaynakIp);
                        lvi.SubItems.Add(con.HedefIp);
                        lvi.SubItems.Add(con.Protokol);
                        lvi.SubItems.Add(con.Durum);
                        listView.Items.Add(lvi);
                        if (con.ProgramPath != null)
                        {
                            lvi.SubItems.Add(con.ProgramPath);
                            Icon ico = Icon.ExtractAssociatedIcon(con.ProgramPath);
                            Image img = ico.ToBitmap();
                            imageList.Images.Add(img);
                            lvi.ImageIndex = imageIndex++;
                        }
                        else
                        {
                            lvi.ImageIndex = 0;
                        }


                        baglantilar.Add(con);
                    }
                }
            }
            cmd.Close();
        }

        private bool listedeVarmi(Connection con)
        {
            if (con.HedefIp.StartsWith("[::1]"))
                return true;

          string hedefIp = con.HedefIp.Split(':')[0];
          return baglantilar.Any(c => c.KaynakIp == con.KaynakIp && c.HedefIp.Split(':')[0] == hedefIp && c.Pid == con.Pid);
        }

        private Connection ayikla(string[] commandLines)
        {
            var con = new Connection();
            foreach (var item in commandLines)
            {
                if (con.Protokol == null && item != "")
                    con.Protokol = item;
                else if (con.KaynakIp == null && item != "")
                    con.KaynakIp = item;
                else if (con.HedefIp == null && item != "")
                    con.HedefIp = item;
                else if (con.Durum == null && (item == "ESTABLISHED" || item == "SYN_SENT"))
                    con.Durum = item;
                else if (con.Pid == 0 && item != "")
                    con.Pid = int.Parse(item);
            }
            program(con);
            return con;
        }
        private void program(Connection con)
        {
            var processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (process.Id == con.Pid)
                {
                    con.Program = process.ProcessName;
                    try
                    {
                        con.ProgramPath = process.MainModule.FileName;
                    }
                    catch { }
                }

            }
        }

        private void tmrBaglantiYenile_Tick(object sender, EventArgs e)
        {
            listele();
        }
    }
}
