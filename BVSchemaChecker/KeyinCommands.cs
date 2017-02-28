//---------------------------------------------------------------------------------------
// $Workfile: KeyinCommands.cs $ : 
// implementation of the keyin commands.
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
 * $Archive: /MDL/BVSchemaChecker/KeyinCommands.cs $
 * $Revision: 1 $
 * $Modtime: 2/24/17 8:20a $
 * $History: KeyinCommands.cs $
 * 
 * *****************  Version 1  *****************
 * User: Mark.anderson Date: 2/24/17    Time: 9:24a
 * Created in $/MDL/BVSchemaChecker
 * Initial VSS commit of the schema checker source code.
 * 
 */
//----------------------------------------------------------------------------------------

using System;
using System.Text;
using BCOM = Bentley.Interop.MicroStationDGN;
using BMI = Bentley.MicroStation.InteropServices;
using ECSR = Bentley.ECSystem.Repository;
using ECSS = Bentley.ECSystem.Session;
using PWAPI = PwApiWrapper;
using System.Collections.Generic;

namespace BVSchemaChecker
{
/// <summary>Class used for running key-ins.  The key-ins
/// XML file commands.xml provides the class name and the method names.
/// </summary>
internal class KeyinCommands
{
    private static int m_iProjectID { get; set; }
    private static bool m_silentMode { get; set; }
    private static bool m_debugMode { get; set; }
    private static SchemaUtilityForm m_selectorForm { get; set; }
    private static bool m_usesSelector { get; set; }

    /// <summary>
    /// this is used to parse out the keyin string.
    /// </summary>
    /// <param name="parsed">the array built from the single string.</param>
    /// <param name="unparsed">the string passed along with the keyin command.</param>
    private static void ParseKeyin(string[] parsed, string unparsed)
    {
        string strProjectID;
        string strProcModels;
        string strSilentMode;
        string strDebugMode;

        parsed = unparsed.Split(' ');
        foreach (string pString in parsed)
        {
            if (pString.StartsWith("-p:"))
            {
                int pos = pString.IndexOf(':');
                strProjectID = pString.Substring(pos+1);
                m_iProjectID = int.Parse(strProjectID);
            }

            if (pString.StartsWith("-m:"))
            {
                int pos = pString.IndexOf(':');
                strProcModels = pString.Substring(pos+1);
                BVSchemaChecker.s_processAllModels =(strProcModels.ToUpper().CompareTo("ALL") == 0);
            }
            if (pString.StartsWith("-s:"))
            {
                int pos = pString.IndexOf(':');
                strSilentMode = pString.Substring(pos + 1);
                m_silentMode = !(strSilentMode.ToUpper().CompareTo("TRUE") == 0);
            }
            if (pString.StartsWith("-d:"))
            {
                int pos = pString.IndexOf(':');
                strDebugMode = pString.Substring(pos + 1);
                m_debugMode = (strDebugMode.ToUpper().CompareTo("TRUE") == 0);
            }
        }
    }
    public static void BVSchemaCheckerProjectControl(System.String unparsed)
    {
        string[] parsed = null;
        //break up the unparsed string
        ParseKeyin(parsed, unparsed);
        m_selectorForm = new SchemaUtilityForm(BVSchemaChecker.MyAddin);
        m_usesSelector = true;
        m_selectorForm.Show();
    }
    /// <summary>
    /// The command to process an entire PW tree.  
    /// </summary>
    /// <param name="unparsed">The arguments that are used to control the command.
    /// -m:ALL to process all the models - by default only the default model will be processed.
    /// -p:projectID - the id of the PW folder to  process.  
    /// if not present then a dialog to select will be shown.
    /// -d:TRUE to provide logging information for debugging
    /// </param>
    public static void BVSchemaCheckerTraverseRepository(System.String unparsed)
    {
        m_silentMode = true; //this will be turned off in the parser...
        string[] parsed=null;
        LogWriter.CreateLog(string.Format("SC_{0}.log", DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")));

        BVSchemaChecker.s_runningTraverse = true;
        
        //break up the unparsed string
        ParseKeyin(parsed, unparsed);

        if (m_debugMode)
        {
            LogWriter.writeLine("in debug mode", true);
        }
        //go through the init logic.
        if (PWAPI.dmscli.aaApi_Initialize(0))
        {
            int status=-1;
            StringBuilder sbName = new StringBuilder(512);
            string sbDataSource;// = new StringBuilder(512);
            string sbPass;// = new StringBuilder(512);
            string sbSchema;// = new StringBuilder(512);
            IntPtr hSession = IntPtr.Zero;
            LogWriter.writeLine("logging into ProjectWise", false);
            if (!PWAPI.dmscli.aaApi_GetCurrentSession(ref hSession) || (hSession == IntPtr.Zero))
                status = PWAPI.dmawin.aaApi_LoginDlg(PWAPI.dmawin.DataSourceType.Unknown,
                                                     sbName, 512, "", "", "");
            else
                LogWriter.writeLine("Already logged into ProjectWise", true);

            if (status == 1)
                LogWriter.writeLine("successfully logged into ProjectWise", true);
        }
        else
            return;

        BVSchemaChecker.iSchemaFoundCount = 0; //initialize the count
        BVSchemaChecker.bUseLogFile = true;
        

        if (m_iProjectID == 0)
            m_iProjectID = PWAPI.dmawin.aaApi_SelectProjectDlg(0, "Select Project",
                                                               m_iProjectID);
        LogWriter.writeLine("selected project", false);

        BVSchemaChecker.BVSchemaCheckerTraverseProject(m_iProjectID);

        BVSchemaChecker.ComApp.MessageCenter.AddMessage(
                            string.Format("Found {0} schema in the Selection", 
                            BVSchemaChecker.iSchemaFoundCount), "Schema Found", 
                            BCOM.MsdMessageCenterPriority.Info, m_silentMode);
        if (m_usesSelector)
            m_selectorForm.SetMessage(
                            string.Format("Found {0} schema in the Selection", 
                            BVSchemaChecker.iSchemaFoundCount));

        LogWriter.writeLine(string.Format("Process Ended at {0}", 
                            DateTime.Now.ToString()), false);
        
        LogWriter.writeLine(string.Format("Processed {0} files and found {1} errors", 
                                          BVSchemaChecker.iFileCount, 
                                          BVSchemaChecker.iSchemaFoundCount), 
                                          true);
        
        LogWriter.close();

        BVSchemaChecker.s_runningTraverse = false;
    }
/// <summary>
/// The command to process the active file.  It will attempt to create a session
/// and open the connection to the active model.  
/// If the connection cannot be opened it will error out.  
/// The findEmbeded method to look at the file for embedded schemas
/// </summary>
/// <param name="unparsed">not used.</param>
    public static void BVSchemaCheckerCommand(System.String unparsed)
    {
        string errMessage = "ERROR";
        List<string> schemaList = new List<string>();
        //set to silent mode
        m_silentMode = false;
        if (1 == BVSchemaChecker.mdlSystem_startedAsAutomationServer() && ((null != unparsed) && unparsed == "FROM_HOOK"))
            return;
        //if an unparsed string is sent in then set silent to true
        if (1!=BVSchemaChecker.mdlSystem_startedAsAutomationServer()&&((null != unparsed) && (unparsed.Length > 0)))
            m_silentMode = true;
        //see if it is an imodel if so then don't check.
        if (1 == BVSchemaChecker.IsIModel(BVSchemaChecker.ComApp.ActiveModelReference.MdlModelRefP()))
        {
            BVSchemaChecker.ComApp.MessageCenter.AddMessage("this is an i-model", "i-models are not processed", BCOM.MsdMessageCenterPriority.Info, false);
            return;
        }
        //there is an imodel attachment process special.
        if (BVSchemaChecker.HasIModelReference(BVSchemaChecker.ComApp.ActiveModelReference))
        {
            BVSchemaChecker.ComApp.MessageCenter.AddMessage("Has IModel Attached", 
                                        "This file has an imodel in the attachment set", 
                                        BCOM.MsdMessageCenterPriority.Info, false);
            BCOM.DesignFile workFile = BVSchemaChecker.ComApp.OpenDesignFileForProgram(
                                        BVSchemaChecker.ComApp.ActiveDesignFile.FullName, true);
            //create a model in the file that has no references
            //proces the new model.  this will avoid using the references as part
            //of the ec repository.
            //delete the model after the processing.
            try
            {
                BCOM.ModelReference workModel = workFile.Models.Add(
                                                workFile.DefaultModelReference, 
                                                "working", "working", 
                                                BCOM.MsdModelType.Normal, true);

                int iAttachmentCount = workModel.Attachments.Count;
                for (int i = iAttachmentCount; i > 0; --i)
                {
                    BCOM.Attachment oAtt = workModel.Attachments[i];
                    workModel.Attachments.Remove(oAtt);
                }

                BVSchemaChecker.ProcessModel(workModel);
                workFile.Models.Delete(workModel);
            }
            catch (Exception e) { Console.WriteLine("unable to wrtie to file"); }
            workFile.Close();
            return;
        }

      //if I am a simple file with normal dgn references
        ECSR.RepositoryConnection connection = null;
        CSVReporter.CreateReport(string.Format("SC_{0}.csv", DateTime.Now.ToString("yyyy-dd-MM")));
        if (m_debugMode)
            BVSchemaChecker.ComApp.MessageCenter.AddMessage("created csv file", 
                string.Format("created report file at {0}", CSVReporter.FileName), 
                BCOM.MsdMessageCenterPriority.Info, m_debugMode);

        if (m_usesSelector)
            m_selectorForm.SetMessage("Processing File " + BVSchemaChecker.ComApp.ActiveDesignFile.Name);
        
        try
        {
            ECSS.ECSession ecSession = ECSS.SessionManager.CreateSession(true);
            connection = BVSchemaChecker.OpenConnectionToActiveModel(ecSession);

            if (BVSchemaChecker.FindEmbededSchemas(connection,schemaList))
            {
                BVSchemaChecker.ComApp.MessageCenter.AddMessage("Found schema in this file. \n Please contact the Bentley Execution team with the Schema list provided on the following dialog box. \n Select Ok to get a list of the schema you will need to provide", 
                                                                "Found Schema", 
                                                                BCOM.MsdMessageCenterPriority.Error,
                                                                m_silentMode);
                BVSchemaChecker.iSchemaFoundCount++;
                //show the list...
                if (m_silentMode)
                {
                    SchemaListForm sform = new SchemaListForm();
                    sform.SetSchemaNames(schemaList);
                    sform.ShowDialog();
                }

            }
            else
                BVSchemaChecker.ComApp.MessageCenter.AddMessage("no schema in the file", 
                                                                "nothing found", 
                                                                BCOM.MsdMessageCenterPriority.Info,
                                                                (unparsed != "FROM_HOOK"));
        }
        catch (Exception e)
        {
            BVSchemaChecker.ComApp.MessageCenter.AddMessage(e.Message, e.Message, 
                                                            BCOM.MsdMessageCenterPriority.Error, 
                                                            !BVSchemaChecker.bUseLogFile);
            errMessage = e.Message;
        }
        finally
        {
            if(null == connection)
                CSVReporter.writeLine(BVSchemaChecker.strCurrentProjectName, 
                                      BVSchemaChecker.ComApp.ActiveDesignFile.Name, 
                                      BVSchemaChecker.ComApp.ActiveModelReference.Name, 
                                      errMessage, false);
            else
                BVSchemaChecker.CloseConnection(connection);

            CSVReporter.close();
        }

    } 
    /// <summary>
    /// a command to append more entries to the whitelist.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void AddToWhiteList(string unparsed)
    {
        string whiteListAddition="";
        StringBuilder fullList = new StringBuilder();
        
        if (unparsed.Length < 0)
            return;
        else
        {
            whiteListAddition = unparsed;
        
            fullList.Append(whiteListAddition);
        }
        //build the new list
        if(BVSchemaChecker.s_whiteList.Count>0)
            foreach(string entry in BVSchemaChecker.s_whiteList)
                fullList.Append(":"+entry);

        //now put the addition on the collection
        BVSchemaChecker.s_whiteList.Add(whiteListAddition);

        //this will update the list for this user.
        BVSchemaChecker.ComApp.ActiveWorkspace.AddConfigurationVariable("BV_SCHEMA_WHITELIST", fullList.ToString(), true);
    }
    /// <summary>
    /// Add the on close event handler
    /// this is available to allow the config to be set to not check for AS
    /// </summary>
    /// <param name="unparsed"></param>
    public static void AddEventHandler(string unparsed)
    {
        Events.SetEventHandlers();
    }
    public static void RemoveEventHandler(string unparsed)
    {
        Events.RemoveEventHandlers();
    }
    public static void About(string unparsed)
    {
        AboutForm aform = new AboutForm(BVSchemaChecker.MyAddin);
        aform.ShowDialog();
    }
}  // End of KeyinCommands

}  // End of the namespace
