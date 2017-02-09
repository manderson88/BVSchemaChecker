//---------------------------------------------------------------------------------------
// $Workfile: $ : implementation of the log processing
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
 * $Archive: $
 * $Revision:  $
 * $Modtime:  $
 * $History:$
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
    /// A class to handle logging the application messages.
    /// </summary>
   public static class LogWriter
   {
      static System.IO.StreamWriter strWriter;
      static string s_fileName;
      /// <summary>
      /// Create the log file in the MS_TMP directory.
      /// </summary>
      /// <param name="fileName">The name of the log file.</param>
      public static void CreateLog(string fileName) // use uStation API to get the uStation temp directory
      {
         string filePath = BVSchemaChecker.ComApp.ActiveWorkspace.ExpandConfigurationVariable("$(MS_TMP)");
         
         // open new file or overwrite existing one
         strWriter = new StreamWriter(File.Open(filePath+fileName, FileMode.Create));
         strWriter.WriteLine("Name: " + fileName);
         strWriter.WriteLine("Created: {0} {1}", DateTime.Now.ToLongDateString(), 
                             DateTime.Now.ToLongTimeString());
         strWriter.WriteLine("----------------------------------");
         s_fileName = filePath+fileName;
      }

      /// <summary>
      /// write an entry into the log file.  optionally the time stamp can be suppressed.
      /// </summary>
      /// <param name="msg"></param>
      /// <param name="bStamp"></param>
      public static void writeLine(string msg, bool bStamp = true)
      {
        //see if there is a strWriter if not then create it again.
          if (null == strWriter)
              strWriter = File.AppendText(s_fileName);
         
         if (bStamp)
            strWriter.WriteLine(DateTime.Now.ToLongTimeString());
         strWriter.WriteLine(msg);
      }

      /// <summary>
      /// close the log file with one final message.
      /// </summary>
      public static void close()
      {
         strWriter.WriteLine("Closed: {0} {1}", DateTime.Now.ToLongDateString(), 
                             DateTime.Now.ToLongTimeString());
         strWriter.Close();
         strWriter = null;
      }
   }
}
