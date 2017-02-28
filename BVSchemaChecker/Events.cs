//---------------------------------------------------------------------------------------
// $Workfile: Events.cs $ : implements the event hooks
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
 * $Archive: /MDL/BVSchemaChecker/Events.cs $
 * $Revision: 1 $
 * $Modtime: 2/24/17 8:16a $
 * $History: Events.cs $
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
using System.Diagnostics;

namespace BVSchemaChecker
{
    internal sealed class Events
    {
        private static Bentley.MicroStation.AddIn.NewDesignFileEventHandler
                                                s_managedEventHandler;

        private static Bentley.MicroStation.AddIn.ElementChangedEventHandler
                                                s_managedChangeHandler;

        private static BVSchemaChecker.HandleElementWriteEvent s_mdlElementWriteToFileHandler;

        ///<summary>Prior to using any mdl..._setFunction 
        ///function we must be certain the current MDL descriptor is
        ///belongs to our add-in. This is essential because the 
        ///mdl...setFunction functions save the function pointer in the
        ///MDL descriptor. This is guaranteed to be true if called from 
        ///directly or indirectly from AddInMain.Run or from a key-in.  
        ///</summary>
        private static void VerifyMdlDescriptor()
        {
            System.IntPtr myMdlDesc = BVSchemaChecker.MyAddin.GetMdlDescriptor();
            System.IntPtr currMdlDesc = BVSchemaChecker.mdlSystem_getCurrMdlDesc();

            System.Diagnostics.Debug.Assert(myMdlDesc == currMdlDesc);
        }

        internal static int HandleElementWriteEvent(
            int action,
            long pModelRef,
            UInt32 filePos,
            IntPtr newEdP,
            IntPtr oldEdP,
            IntPtr  replacementEdP)
        {
            Debug.Print("writing to file");
            return 0;
        }

        
        internal static void SetEventHandlers()
        {
       // s_managedChangeHandler =
       //     new Bentley.MicroStation.AddIn.ElementChangedEventHandler(BVSchemaChecker.BCSchemaElementEventHandler);

       //     BVSchemaChecker.MyAddin.ElementChangedEvent += s_managedChangeHandler;
        //BVSchemaChecker.mdlSystem_setElementWriteToFileFunction(17,)
        s_managedEventHandler =
            new Bentley.MicroStation.AddIn.NewDesignFileEventHandler(BVSchemaChecker.BVSchemFileEventHandler);

        BVSchemaChecker.MyAddin.NewDesignFileEvent += s_managedEventHandler;       
        }
        internal static void RemoveEventHandlers()
        {
            if(null != s_managedEventHandler)
                BVSchemaChecker.MyAddin.NewDesignFileEvent -= s_managedEventHandler;
        }
    }
}
