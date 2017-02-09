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
    public partial class SchemaListForm :
#if ADDIN
      Bentley.MicroStation.WinForms.Adapter
#else
      Form
#endif

    {
       
        public SchemaListForm()
        {
            InitializeComponent();
        }
        public void SetSchemaNames(List<string> values)
        {
            foreach (string name in values)
                lstBxSchemas.Items.Add(name);
        }
    }
}
