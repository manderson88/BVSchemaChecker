//---------------------------------------------------------------------------------------
// $Workfile: CSVReporter.cs $ : implementation of the form
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
 * $Archive: /MDL/BVSchemaChecker/CSVReporter.cs $
 * $Revision: 1 $
 * $Modtime: 2/09/17 7:34a $
 * $History: CSVReporter.cs $
 * 
 * *****************  Version 1  *****************
 * User: Mark.anderson Date: 2/24/17    Time: 9:24a
 * Created in $/MDL/BVSchemaChecker
 * Initial VSS commit of the schema checker source code.
 * 
 */
//----------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace BVSchemaChecker
{
    /// <summary>
    /// A class to handle the report writing.
    /// </summary>
    static class CSVReporter
    {
        static System.IO.StreamWriter strWriter;
        public static string FileName { get; set; } 
        /// <summary>
        /// Creates the report file.  The location is based on the cfg var
        /// BV_SCHEMACHECKER or if that is not set then MS_TMP.  
        /// </summary>
        /// <param name="fileName">The name for the report file</param>
        public static void CreateReport(string _fileName) // use uStation API to get the uStation temp directory
        {
            bool bOverrideOutput;
            bOverrideOutput = BVSchemaChecker.ComApp.ActiveWorkspace.IsConfigurationVariableDefined("BV_SCHEMACHECKER_OUT");
            string filePath;

            if (bOverrideOutput)
                filePath = BVSchemaChecker.ComApp.ActiveWorkspace.ExpandConfigurationVariable("$(BV_SCHEMACHECKER_OUT)");
            else
                filePath = BVSchemaChecker.ComApp.ActiveWorkspace.ExpandConfigurationVariable("$(MS_TMP)");
            //set the property for debug
            FileName = filePath + _fileName;
            // open new file or overwrite existing one
            try
            {
                strWriter = File.AppendText(FileName);
            }
            catch (Exception e)
            {
                BVSchemaChecker.ComApp.MessageCenter.AddMessage
                    ("Exception Writing Log File", 
                    string.Format("Exception when writing to {0}", FileName), 
                    Bentley.Interop.MicroStationDGN.MsdMessageCenterPriority.Error, 
                    true);
            }
        }

        /// <summary>
        /// Write an entry in the report file.
        /// The format is
        /// Store name, file name, model name, schema information, date processed, fail or pass
        /// </summary>
        /// <param name="strPWStore">The PW store</param>
        /// <param name="strFileName">The file name being processed.</param>
        /// <param name="strModelName">The model being processed.</param>
        /// <param name="strSchemaInfo">The schema that is found.</param>
        /// <param name="bPassFail">wether to put in a PASS or FAIL message.</param>
        public static void writeLine(string strPWStore, string strFileName, 
                string strModelName, string strSchemaInfo, bool bPassFail = true)
        {
            if (null == strWriter)
                strWriter = File.AppendText(FileName);

            if (!bPassFail)
                strWriter.WriteLine(string.Format("{0},{1},{2},{3},{4},FAIL", 
                    strPWStore, strFileName, strModelName, strSchemaInfo, 
                    DateTime.Now.ToLongTimeString()));
            else
                strWriter.WriteLine(string.Format("{0},{1},{2},{3},{4},PASS", 
                    strPWStore, strFileName, strModelName, strSchemaInfo, 
                    DateTime.Now.ToLongTimeString()));

        }
       
        /// <summary>
        /// close the file
        /// </summary>
        public static void close()
        {
            strWriter.Close();
            strWriter = null;
        }

    }
}
