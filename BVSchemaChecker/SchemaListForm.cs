//---------------------------------------------------------------------------------------
// $Workfile: SchemaListForm.cs $ : implementation of the form
//
// --------------------------------------------------------------------------------------
// NOTICE TO ALL PERSONS HAVING ACCESS HERETO:  This document or recording contains 
// computer software or related information constituting proprietary trade secrets of 
// Black & Veatch Holding Company, which have been maintained in "unpublished" status 
// under the copyright laws, and which are to be treated by all persons having access 
// thereto in manner to preserve the status thereof as legally protectable trade secrets 
// by neither using nor disclosing the same to others except as may be expressly 
// authorized in advance by Black & Veatch Holding Company.  However, it is intended
// that all prospective rights under the copyright laws in the event of future 
// "publication" of this work shall also be reserved; for which purpose only, the 
// following is included in this notice, to wit, 
// "(C) COPYRIGHT 2016 BY BLACK & VEATCH HOLDING COMPANY, ALL RIGHTS RESERVED"
//---------------------------------------------------------------------------------------
/*
 * CHANGE LOG
 * $Archive: /MDL/BVSchemaChecker/SchemaListForm.cs $
 * $Revision: 3 $
 * $Modtime: 3/29/17 9:57a $
 * $History: SchemaListForm.cs $
 * 
 * *****************  Version 3  *****************
 * User: Mark.anderson Date: 3/29/17    Time: 10:18a
 * Updated in $/MDL/BVSchemaChecker
 * updated the documentation per WPR review.
 * 
 * *****************  Version 2  *****************
 * User: Mark.anderson Date: 3/22/17    Time: 4:08p
 * Updated in $/MDL/BVSchemaChecker
 * updated the format and documentation 
 * 
 * *****************  Version 1  *****************
 * User: Mark.anderson Date: 2/24/17    Time: 9:25a
 * Created in $/MDL/BVSchemaChecker
 * Initial VSS commit of the schema checker source code.
 * 
 */
//----------------------------------------------------------------------------------------
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
/*-----------------------------------------------------------------------------*/
    /// <summary>
    /// This is the form that displays the names of the schema in the file.
    /// </summary>
/*-----------------------------------------------------------------------------*/
   public partial class SchemaListForm :
#if ADDIN  //experimenting with using this preprocessor def to allow for toggling to 
           //get the form to work in design mode.  to test this undefine ADDIN in the
           //the project properties
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
