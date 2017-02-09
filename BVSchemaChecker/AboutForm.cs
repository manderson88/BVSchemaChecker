using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BVSchemaChecker
{
    public partial class AboutForm :
#if ADDIN
      Bentley.MicroStation.WinForms.Adapter
#else
      Form
#endif
    {
        public AboutForm(Bentley.MicroStation.AddIn host)
        {
            InitializeComponent();
        }
    }
}
