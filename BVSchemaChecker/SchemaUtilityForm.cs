//---------------------------------------------------------------------------------------
// $Workfile: SchemaUtilityForm.cs $ : implementation of the form
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
 * $Archive: /MDL/BVSchemaChecker/SchemaUtilityForm.cs $
 * $Revision: 1 $
 * $Modtime: 11/17/16 12:03p $
 * $History: SchemaUtilityForm.cs $
 * 
 * *****************  Version 1  *****************
 * User: Mark.anderson Date: 2/24/17    Time: 9:25a
 * Created in $/MDL/BVSchemaChecker
 * Initial VSS commit of the schema checker source code.
 * 
 */
//----------------------------------------------------------------------------------------
#define ADDIN
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
#region "Bentley Namespaces"
using BMW = Bentley.MicroStation.WinForms;
using BMI = Bentley.MicroStation.InteropServices;
using BCOM = Bentley.Interop.MicroStationDGN;
using BM = Bentley.MicroStation;
using BW = Bentley.Windowing;
using PWAPI = PwApiWrapper;
#endregion

namespace BVSchemaChecker
{
    /// <summary>
    /// the form to show.  This form allows the user to set a base for the sub
    /// selection.  The user can then select sub folders to process.
    /// </summary>
    public partial class SchemaUtilityForm : 
#if ADDIN
      Bentley.MicroStation.WinForms.Adapter
#else
      Form
#endif
    {
        private BW.WindowContent m_windowContent;
        private int m_projectBase;
        List<ListBoxEntry> itemList;
        BindingList<ListBoxEntry> bindingList;
        BindingSource source;
#if !ADDIN
        public SchemaUtilityForm()
        {
            InitializeComponent();
        }
#endif
        /// <summary>
        /// the constructor.  sets the form to be hosted in Bentley products.
        /// </summary>
        /// <param name="addIn">The owner application.</param>
        public SchemaUtilityForm(Bentley.MicroStation.AddIn addIn)
        {
            itemList = new List<ListBoxEntry>();
            bindingList = new BindingList<ListBoxEntry>(itemList);
            source = new BindingSource(bindingList, null);

            InitializeComponent();

            dgvDirs.AllowUserToAddRows = false;
            dgvDirs.RowHeadersVisible = false;
            dgvDirs.DataSource = source;
#if (ADDIN)

            AttachAsTopLevelForm(addIn, true);

            this.NETDockable = false;
            Bentley.Windowing.WindowManager windowManager;
            windowManager = Bentley.Windowing.WindowManager.GetForMicroStation();
            m_windowContent = windowManager.DockPanel(this, this.Name, this.Text, 
                                                      BW.DockLocation.Floating);
            if (PWAPI.dmscli.aaApi_Initialize(0))
            {
                int status;
                StringBuilder sbName = new StringBuilder(512);
                string sbDataSource;// = new StringBuilder(512);
                string sbPass;// = new StringBuilder(512);
                string sbSchema;// = new StringBuilder(512);
                IntPtr hSession = IntPtr.Zero;
                if (!PWAPI.dmscli.aaApi_GetCurrentSession(ref hSession) || 
                    (hSession == IntPtr.Zero))
                    status = PWAPI.dmawin.aaApi_LoginDlg(PWAPI.dmawin.DataSourceType.Unknown, 
                                                         sbName, 512, "", "", "");
            }
#endif
        }
        /// <summary>
        /// set the message string at the base of the dialog.
        /// </summary>
        /// <param name="msg">The message to display at the bottom of the dialog.</param>
        public void SetMessage(string msg)
        {
            lblMessages.Text = msg;
        }
        /// <summary>
        /// The button to set the root project.
        /// </summary>
        /// <param name="sender">the sending application</param>
        /// <param name="e">the args for the button event.</param>
        private void btnRoot_Click(object sender, EventArgs e)
        {
            //open the pwexplorer and select the root.
            int iProjectID = 0;
            //need to find a way to check  to make sure it is a dgn file that is better than just the extension...
            iProjectID = PWAPI.dmawin.aaApi_SelectProjectDlg(0, "Select Root Project Folder", 
                                                             iProjectID);
            string strName;
            if (iProjectID > 0)
            {
                m_projectBase = iProjectID;
                
                PWAPI.dmscli.aaApi_SelectProject(iProjectID);

                strName = PWAPI.dmscli.aaApi_GetProjectStringProperty(
                                          PWAPI.dmscli.ProjectProperty.Name, 0);
                txtRootDir.Text = strName;
            }
        }
        /// <summary>
        /// the button to set the sub folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            int iFolderID;
            iFolderID = PWAPI.dmawin.aaApi_SelectProjectDlg(0, "Select Sub Folder", 
                                                            m_projectBase);
            PWAPI.dmscli.aaApi_SelectProject(iFolderID);

            ListBoxEntry lbEntry = new ListBoxEntry();

            lbEntry.folderID = iFolderID;
            lbEntry.name = PWAPI.dmscli.aaApi_GetProjectStringProperty(
                                          PWAPI.dmscli.ProjectProperty.Name, 0);
            lbEntry.processAllModels = false;
            source.Add(lbEntry);
            dgvDirs.Update();
            //cblbFolders.Items.Add(lbEntry.toString());
        }
        /// <summary>
        /// the button for starting the processing of one or more selected folders.
        /// sends the project id, a flag to process all the models in each file
        /// Silently sends a flag to turn off the message box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            
            foreach (ListBoxEntry lbEntry in source)
            {
                if (lbEntry.processFile)
                {
                    string projectId = string.Format("{0}",lbEntry.folderID);
                    string procAll = "";

                    if (lbEntry.processAllModels)
                        procAll = "ALL";
                    else
                        procAll = "ONE";

                KeyinCommands.BVSchemaCheckerTraverseRepository(
                      string.Format("-p:{0} -m:{1} -s:TRUE",projectId,procAll));
                }
            }
        }
    }
    /// <summary>
    /// a class to allow the data grid view to have a collection 
    /// </summary>
    public class ListBoxEntry
    {
        public bool processFile { get; set; }
        /// <summary>
        /// the name for the item
        /// </summary>
        public String name { get; set; }
        /// <summary>
        /// the PW folder id
        /// </summary>
        public Int32 folderID { get; set; }
        /// <summary>
        /// a flag to process all the models.
        /// </summary>
        public bool processAllModels { get; set; }
    }
}
