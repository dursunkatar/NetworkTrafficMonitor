using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetViewer
{
    public class Connection
    {
        public string Protokol { get; set; }
        public string KaynakIp { get; set; }
        public string HedefIp { get; set; }
        public string Program { get; set; }
        public string ProgramPath { get; set; }
        public string Durum { get; set; }
        public int Pid { get; set; }
    }
}
