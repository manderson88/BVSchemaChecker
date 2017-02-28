
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
 * $Revision: 1 $
 * $Modtime: 2/24/17 8:21a $
 * $History: AboutForm.cs $
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
            lblAbout.Text = "Black && Veatch Schema Checker Tool\r\nversion 2.3.2 Feb-27-2017\r\n\r\n For UAT";
        }
    }
}
