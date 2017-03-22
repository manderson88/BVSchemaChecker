//---------------------------------------------------------------------------------------
// $Workfile: AboutForm.cs $ : implementation of the about form
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
 * $Archive: /MDL/BVSchemaChecker/AboutForm.cs $
 * $Revision: 3 $
 * $Modtime: 3/15/17 11:52a $
 * $History: AboutForm.cs $
 * 
 * *****************  Version 3  *****************
 * User: Mark.anderson Date: 3/15/17    Time: 12:43p
 * Updated in $/MDL/BVSchemaChecker
 * updating the command for toggling the write to file hook.  fixing up
 * the imodel detection and updating the comments.
 * 
 * *****************  Version 2  *****************
 * User: Mark.anderson Date: 3/06/17    Time: 12:18p
 * Updated in $/MDL/BVSchemaChecker
 * updated version number
 * 
 * *****************  Version 1  *****************
 * User: Mark.anderson Date: 2/24/17    Time: 9:24a
 * Created in $/MDL/BVSchemaChecker
 * Initial VSS commit of the schema checker source code.
 * 
 */
//----------------------------------------------------------------------------------------using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BVSchemaChecker
{
    /// <summary>
    /// a simple form to dispplay the version information.
    /// this should have the version number updated for each release.
    /// </summary>
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
            lblAbout.Text = "Black && Veatch Schema Checker Tool\r\nversion 2.3.4 March-06-2017\r\n\r\n Released";
        }
    }
}
