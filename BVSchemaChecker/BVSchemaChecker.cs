//---------------------------------------------------------------------------------------
// $Workfile: BVSchemaChecker.cs $ : 
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
 * $Archive: /MDL/BVSchemaChecker/BVSchemaChecker.cs $
 * $Revision: 6 $
 * $Modtime: 3/29/17 9:45a $
 * $History: BVSchemaChecker.cs $
 * 
 * *****************  Version 6  *****************
 * User: Mark.anderson Date: 3/29/17    Time: 10:17a
 * Updated in $/MDL/BVSchemaChecker
 * updated the documentation per WPR review.
 * 
 * *****************  Version 5  *****************
 * User: Mark.anderson Date: 3/22/17    Time: 4:08p
 * Updated in $/MDL/BVSchemaChecker
 * updated the format and documentation 
 * 
 * *****************  Version 4  *****************
 * User: Mark.anderson Date: 3/15/17    Time: 12:43p
 * Updated in $/MDL/BVSchemaChecker
 * updating the command for toggling the write to file hook.  fixing up
 * the imodel detection and updating the comments.
 * 
 * *****************  Version 3  *****************
 * User: Mark.anderson Date: 3/09/17    Time: 11:27a
 * Updated in $/MDL/BVSchemaChecker
 * clean up code add try catch on target invoke issue in the imodel check.
 * 
 * *****************  Version 2  *****************
 * User: Mark.anderson Date: 3/06/17    Time: 12:20p
 * Updated in $/MDL/BVSchemaChecker
 * updated version information added white list
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
//  Provides access to adapters needed to use forms and controls
//  from System.Windows.Forms in MicroStation
using BMW=Bentley.MicroStation.WinForms;
//  Provides access to classes used to make forms dockable in MicroStation
using BWW=Bentley.Windowing.WindowManager;
//  The Primary Interop Assembley (PIA) for MicroStation's COM object
//  model uses the namespace Bentley.Interop.MicroStationDGN
using BCOM=Bentley.Interop.MicroStationDGN;
//  The InteropServices namespace contains utilities to simplify using 
//  COM object model.
using BMI=Bentley.MicroStation.InteropServices;
using ECSR = Bentley.ECSystem.Repository;
using ECS = Bentley.ECSystem;
using ECO = Bentley.ECObjects;
using ECOS = Bentley.ECObjects.Schema;
using ECPS = Bentley.EC.Persistence;
using ECPQ = Bentley.EC.Persistence.Query;
using EC = Bentley.EC.Plugin;
using BDGNP = Bentley.DGNECPlugin;
using BFSRP = Bentley.FsrECPlugin;
using BPlugin = Bentley.MyComputerECPlugin;
using ECSS = Bentley.ECSystem.Session;
using BECP = Bentley.ECPlugin;
using PWAPI = PwApiWrapper;
using SRI = System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace BVSchemaChecker
{
/*-----------------------------------------------------------------------------*/
/// <summary>This class subclasses the Bentley.MicroStation.AddIn class.  The 
/// AddIn class is a managed implementation of the MDL application. The BVSchemaChecker
/// class provides the base functionality.  It processes the models checks for 
/// schema in the model.  It can detect the host product to avoid some false 
/// positive results.  It uses support from the WPAHelper application for controlling
/// selecting elements from i-models that can cause schema to be embeded into the
/// host dgn file.
/// When loading an AddIn MicroStation looks for a class
/// derived from AddIn.</summary>
/*-----------------------------------------------------------------------------*/
   [Bentley.MicroStation.AddInAttribute(MdlTaskID="BVSchemaChecker", 
                                       KeyinTree="BVSchemaChecker.commands.xml")]
   internal sealed class BVSchemaChecker : Bentley.MicroStation.AddIn
   {
      /// <summary>
      /// this is the write to file callback function signature
      /// </summary>
      /// <param name="action">the event description</param>
      /// <param name="pModelRef">the model reference</param>
      /// <param name="filePos">the file position of the element</param>
      /// <param name="newEdP">the new element</param>
      /// <param name="oldEdP">the old element</param>
      /// <param name="replacementEdP">the replacement</param>
      internal delegate void HandleElementWriteEvent(int action, long pModelRef, 
                                                     UInt32 filePos, 
                                                     IntPtr newEdP, 
                                                     IntPtr oldEdP, 
                                                     IntPtr replacementEdP);

/*-----------------------------------------------------------------------------*/      
      /// <summary>
      /// returns the element type number.  It is a managed call wrapper around
      /// the native code function call elementRef_getType
      /// </summary>
      /// <param name="elRef"></param>
      /// <returns></returns>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("stdmdlbltin.dll", EntryPoint = "elementRef_getType",
      CharSet = SRI.CharSet.Ansi,
      CallingConvention = SRI.CallingConvention.StdCall)]
      internal static extern int elementRef_getType(long elRef);

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// this is for a write to file event
      /// </summary>
      /// <param name="functionType"></param>
      /// <param name="eventDelegate"></param>
      /// <returns></returns>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("USTATION.DLL", EntryPoint = "mdlSystem_setFunction",
                  CallingConvention = SRI.CallingConvention.Cdecl)]
      internal static extern System.IntPtr mdlSystem_setElementWriteToFileFunction(
                     int functionType, HandleElementWriteEvent eventDelegate);

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// this gets the current mdl descriptor for the callbacks
      /// </summary>
      /// <returns></returns>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("USTATION.DLL", CharSet = SRI.CharSet.Ansi,
                  CallingConvention = SRI.CallingConvention.Cdecl)]
      internal static extern System.IntPtr mdlSystem_getCurrMdlDesc();

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// a wrapper for the MDL function to load reference models.  added for 
      /// the work file functionality.  See the MDL Function Reference for complete 
      /// documentation.
      /// </summary>
      /// <param name="pModel">the model reference to load to</param>
      /// <param name="loadCache">to force the loading of element cache</param>
      /// <param name="loadRaster">to force the loading of raster reference files</param>
      /// <param name="loadUndisp">to force the loading of non displayed information</param>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("USTATION.DLL", EntryPoint = "mdlModelRef_loadReferenceModels",
         CharSet= SRI.CharSet.Ansi,
         CallingConvention = SRI.CallingConvention.Cdecl)]
      internal static extern void mdlModelRef_loadReferenceModels(
         int pModel, int loadCache, int loadRaster, int loadUndisp);

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// this is added to catch write to file events.  It is a native code call
      /// to use the ElementAgenda class.  Important note is that the WPAHelper
      /// application needs to be compiled in VisualStudio 2005 only!!! 
      /// </summary>
      /// <param name="bSilent"></param>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("WPAHelper.dll", EntryPoint = "addWriteToFileHook",
         CharSet = SRI.CharSet.Ansi, CallingConvention = SRI.CallingConvention.Cdecl)]
      internal static extern void mdlWriteToFileHook(int bSilent);

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// Removes the write to file hook. 
      /// </summary>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("WPAHelper.dll", EntryPoint = "removeWriteToFileHook",
         CharSet = SRI.CharSet.Ansi, CallingConvention = SRI.CallingConvention.Cdecl)]
      internal static extern void mdlDropWriteToFileHook();

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// use this to check to see if the application is running as an AutomationService client
      /// not too sure how well it is working.
      /// </summary>
      /// <returns></returns>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("USTATION.DLL", EntryPoint = "mdlSystem_startedAsAutomationServer",
         CallingConvention = SRI.CallingConvention.Cdecl, CharSet = SRI.CharSet.Ansi)]
      internal static extern int mdlSystem_startedAsAutomationServer();

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// tells if the model is an imodel. 1 it is 0 it is not.
      /// </summary>
      /// <param name="pModel"></param>
      /// <returns>1 for imodel 0 for  regular</returns>
/*-----------------------------------------------------------------------------*/
      [SRI.DllImport("WPAHelper.dll", EntryPoint = "isIModel",
            CharSet = SRI.CharSet.Ansi,
         CallingConvention = SRI.CallingConvention.Cdecl)]
      internal static extern int IsIModel(int pModel);

      //a reference to addin to be used via the method for getting and passing to
      // the user interface methods.
      private static BVSchemaChecker            s_addin = null;
      //a reference to the com object that is used by some methods.
      private static BCOM.Application           s_comApp = null;
      //an event handler
      public static  ElementChangedEventHandler BCSchemaElementEventHandler;
      //the counter for how many schema have been found 
      public static  int                        iSchemaFoundCount { set; get; }
      //keeps a count of number of files processed in the batch mode.
      public static  int                        iFileCount { set; get; }
      //a flag to tell if we are using a log file (needed since logs get written
      public static  bool                       bUseLogFile { get; set; }
      //keep track of what the currnt PW project name is.
      public static  string                     strCurrentProjectName { get; set; }
      //a flag to tell if we are in Ustation.
      public static  bool                       s_bIsUSTN { get; set; }
      //a flag to tell if we are in OpenPlant
      public static  bool                       s_bIsOPM { get; set; }
      //a flag to tell if we are processing all models in the file.
      public static  bool                       s_processAllModels { get; set; }
      //the list of accepted schema
      public static  List<string>               s_whiteList { get; set; }
      //running the traverse command set by the keyin command.
      public static  bool                       s_runningTraverse { get; set; }

/*-----------------------------------------------------------------------------*/
      /// <summary>Private constructor required for all AddIn classes derived from 
      /// Bentley.MicroStation.AddIn.
      /// </summary>
      /// <param name="mdlDesc">a pointer to the mdl app behind the addin</param>
/*-----------------------------------------------------------------------------*/
      private BVSchemaChecker(System.IntPtr mdlDesc)
         : base(mdlDesc)
      {
         s_addin = this;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>The AddIn loader creates an instance of a class 
      /// derived from Bentley.MicroStation.AddIn and invokes Run.
      /// </summary>
      /// <param name="commandLine">arguments sent in with the mdl load command
      /// </param>
/*-----------------------------------------------------------------------------*/
      protected override int Run(System.String[] commandLine)
      {
         s_comApp = BMI.Utilities.ComApp;
         s_bIsOPM = (s_comApp.Name.CompareTo("OpenPlantModeler")==0||s_comApp.Name.CompareTo("BRCM") == 0);
         s_bIsUSTN = (s_comApp.Name.CompareTo("ustation") == 0||
            s_comApp.Name.CompareTo("AECOsimBuildingDesigner")==0);
         
         s_comApp.CadInputQueue.SendKeyin("mdl load wpahelper");
         s_whiteList = new List<string>();
         string whiteList="";
      
         //check to see if there is a white list of acceptable schema. This is stored
         //in the config file.  There is a command to build the list.
         if(s_comApp.ActiveWorkspace.IsConfigurationVariableDefined("BV_SCHEMA_WHITELIST"))
            whiteList= s_comApp.ActiveWorkspace.ConfigurationVariableValue("BV_SCHEMA_WHITELIST", true);

         string[] values;
         if (whiteList.Length > 0)
         {
            values = whiteList.Split(':');

            foreach (string value in values)
                  s_whiteList.Add(value);
         }

         s_runningTraverse = false;

         //this.ElementChangedEvent += new ElementChangedEventHandler(BVSchemaElementEventHandler);
         //native code write to file hooks.
         //this looks for the write to file options 
         //and for the input queue
         int bSilent = 0;
         if (s_comApp.ActiveWorkspace.IsConfigurationVariableDefined("BV_SCHEMACHECKER_SILENT"))
            bSilent = int.Parse(s_comApp.ActiveWorkspace.ConfigurationVariableValue("BV_SCHEMACHECKER_SILENT",false));
         
         //this replaces the call to just turn on the hook. so we only have one
         //path through this operation.  should possibly use a cfg var at some point.
         TurnOnWriteHook(true);
         //mdlWriteToFileHook(bSilent);
            
         //this sets the on close the var must be defined and have a value of 1 to set the close hook
         if (s_comApp.ActiveWorkspace.IsConfigurationVariableDefined("BV_SCHEMACHECKER_ONCLOSE")&&
            1==int.Parse(s_comApp.ActiveWorkspace.ConfigurationVariableValue("BV_SCHEMACHECKER_ONCLOSE",false)))
               Events.SetEventHandlers();

         return 0;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>Static property that the rest of the application uses 
      /// to get the reference to the AddIn.
      /// </summary>
      /// <returns>a reference to the AddIn subclass</returns>
/*-----------------------------------------------------------------------------*/
      internal static BVSchemaChecker MyAddin { get { return s_addin; } }

/*-----------------------------------------------------------------------------*/
      /// <summary>Static property that the rest of the application uses to
      /// get the reference to the COM object model's main application object.
      /// </summary>
      /// <returns>the com app that is the host platform</returns>
/*-----------------------------------------------------------------------------*/
      internal static BCOM.Application ComApp { get { return s_comApp; } }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// turns off and on the write to file hook.  This is used to block the write
      /// to file operations.  This is called from the 
      /// command.  "BVSCHEMACHECKER TOGGLE ON/OFF
      /// </summary>
      /// <param name="onOffStatus">true turns on false turns off</param>
/*-----------------------------------------------------------------------------*/
      public static void TurnOnWriteHook(bool onOffStatus)
      {
         int bSilent = 0;
         if (s_comApp.ActiveWorkspace.IsConfigurationVariableDefined("BV_SCHEMACHECKER_SILENT"))
               bSilent = int.Parse(s_comApp.ActiveWorkspace.ConfigurationVariableValue("BV_SCHEMACHECKER_SILENT", false));
            
         if (true == onOffStatus)
         {
            mdlWriteToFileHook(bSilent);
                
            ComApp.MessageCenter.AddMessage("write hook enabled", 
                                          "The Write to file hooks are on", 
                                          BCOM.MsdMessageCenterPriority.Info, 
                                          false);
         }
         else
         {
            mdlDropWriteToFileHook();

            ComApp.MessageCenter.AddMessage("write hook disabled", 
                                          "The Write to file hooks are off",  
                                          BCOM.MsdMessageCenterPriority.Info, 
                                          false);
         }
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>Closes a connection. Always close the connection before opening a new file.
      /// </summary>
      /// <param name="connection">the connetion to close</param>
/*-----------------------------------------------------------------------------*/
      public static void CloseConnection(ECSR.RepositoryConnection connection)
      {
         ECSR.RepositoryConnectionService repositoryConnectionService;
         repositoryConnectionService = ECSR.RepositoryConnectionServiceFactory.GetService();
         repositoryConnectionService.Close(connection,null);
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// open an EC connection to the  current file.
      /// </summary>
      /// <param name="ecSession">the EC session reference</param>
      /// <returns>RepositoryConnection</returns>
/*-----------------------------------------------------------------------------*/
      public static ECSR.RepositoryConnection OpenConnectionToActiveModel(ECSS.ECSession ecSession)
      {
         ECSR.RepositoryConnectionService repositoryConnectionService;
         repositoryConnectionService = ECSR.RepositoryConnectionServiceFactory.GetService();

         string fileName = BVSchemaChecker.ComApp.ActiveDesignFile.FullName; // implies active file
         string modelName = BVSchemaChecker.ComApp.ActiveModelReference.Name; // implies active model
         //string loc;
         string location;
         location = BECP.Common.ECRepositoryConnectionHelper.BuildLocation(
                                                         fileName, modelName);

         string ecPluginId = BDGNP.Constants.PluginID;
#if DEBUG
         System.Diagnostics.Debug.WriteLine("location = " + location);
         System.Diagnostics.Debug.WriteLine("PluginID = " + ecPluginId);
#endif
         ECSR.RepositoryConnection connection;
         connection = repositoryConnectionService.Open(ecSession, ecPluginId, 
                                                         location, null, null);

         string fsrPlugin = BFSRP.FSRClientHelper.FSRPluginId;

         return connection;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// open a connection to a model does not have to be the active model
      /// </summary>
      /// <param name="oModel">the model that is being used as repository</param>
      /// <param name="ecSession">the session</param>
      /// <returns></returns>
/*-----------------------------------------------------------------------------*/
      public static ECSR.RepositoryConnection OpenConnectionToModel(
                           BCOM.ModelReference oModel, ECSS.ECSession ecSession)
      {
         ECSR.RepositoryConnectionService repositoryConnectionService;
         repositoryConnectionService = ECSR.RepositoryConnectionServiceFactory.GetService();

         string fileName = oModel.DesignFile.FullName; // implies active file
         string modelName = oModel.Name; // implies active model
         string location;
         location = BECP.Common.ECRepositoryConnectionHelper.BuildLocation(
                                                         fileName, modelName);

         string ecPluginId = BDGNP.Constants.PluginID;
#if DEBUG
         System.Diagnostics.Debug.WriteLine("location = " + location);
         System.Diagnostics.Debug.WriteLine("PluginID = " + ecPluginId);
#endif
         ECSR.RepositoryConnection connection;
         connection = repositoryConnectionService.Open(ecSession, ecPluginId,
                                                         location, null, null);
         System.Diagnostics.Debug.Assert(null != connection);

         string fsrPlugin = BFSRP.FSRClientHelper.FSRPluginId;

         return connection;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// sees if the schema name is on a white list.
      /// the white list is set by the cfg var BV_SCHEMA_WHITELIST.
      /// </summary>
      /// <param name="schemaName">the name(without the version information) of a schema to check</param>
      /// <returns>true the item is on the white list.</returns>
/*-----------------------------------------------------------------------------*/
      private static bool IsOnWhiteList(string schemaName)
      {
         foreach (string whiteSchema in s_whiteList)
            if (schemaName.CompareTo(whiteSchema)==0)
               return true;

         return false;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// if the  application is being run in OpenPlant Modeler the OpenPlant
      /// and OpenPlant3D schema are always present.  so  skip them.
      /// </summary>
      /// <param name="schemaNames">The list of schema found in the file.</param>
      /// <returns>true the item is an OpenPlant base schema</returns>
/*-----------------------------------------------------------------------------*/
      private static bool IsOPMBaseSchema(string[] schemaNames)
      {
         bool bStatus = false;
         if(s_bIsOPM)
         foreach(string name in schemaNames)
            if (name.CompareTo("OpenPlant.01.06")==0||
                name.CompareTo("OpenPlant_3D.01.06")==0)
                  bStatus = true;

         return bStatus;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// checks for  the  building group schemas that are added
      /// </summary>
      /// <param name="schemaNames">the schema to check</param>
      /// <returns>true the item is in the base set that are used in ABD</returns>
/*-----------------------------------------------------------------------------*/
      private static bool IsABDBaseSchema(string name)
      {
         bool bStatus = false;
         if (s_comApp.Name=="AECOsimBuildingDesigner")
            if(name=="Bentley_SpacePlanner_LocationLinkings"||
               name=="StructuralModelingComponents"||
                  name=="BuildingDataGroup")
                  bStatus = true;
         return bStatus;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// if the schema is one of the OPM base items.
      /// </summary>
      /// <param name="name">the schema to check</param>
      /// <returns>true if the schema is part of the OPM base set.</returns>
/*-----------------------------------------------------------------------------*/
      private static bool IsOPMSchema(string name)
      {
         bool bStatus = false;
         if (s_comApp.Name == "OpenPlantModeler")
            if (name == "OpenPlant" ||
                name == "OpenPlant_3D"||
                name== "StructuralModelingComponents")
                  bStatus = true;

         return bStatus;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// check  for  brcm schema
      /// </summary>
      /// <param name="name">the schema to check</param>
      /// <returns>true the item is in teh BRCM base list.</returns>
/*-----------------------------------------------------------------------------*/
      private static bool IsBRCMSchema(string name)
      {
         bool bStatus = false;
         if (s_comApp.Name == "BRCM")
            if (name == "Electrical_RCM")
               bStatus = true;

         return bStatus;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// looking at possible solution for a MIA file problem.  uses the File 
      /// api to see if the file exists.
      /// </summary>
      /// <param name="pModel">the model to check</param>
      /// <returns>true the file is on the drive</returns>
/*-----------------------------------------------------------------------------*/
      bool findFile(BCOM.ModelReference pModel)
      {
         string filePath = pModel.DesignFile.FullName;
         return File.Exists(filePath);
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// checks to see if there is an imodel.
      /// </summary>
      /// <param name="pModel">the root model to search for references</param>
      /// <returns>true the model has an i-model referenced in.</returns>
/*-----------------------------------------------------------------------------*/
      public static bool HasIModelReference(BCOM.ModelReference pModel)
      {
         try
         {
            if (!pModel.IsAttachment || 
               (pModel.IsAttachment && !pModel.AsAttachment.IsMissingFile))
                  if (1 == IsIModel(pModel.MdlModelRefP()))
                     return true;

            foreach (BCOM.Attachment oAttachment in pModel.Attachments)
            {
               BCOM.ModelReference rModel = (BCOM.ModelReference)oAttachment;

               if (!oAttachment.IsMissingFile)
                  if (1 == IsIModel(rModel.MdlModelRefP()))
                     return true;

               foreach (BCOM.Attachment a in oAttachment.Attachments)
               {
                  return HasIModelReference((BCOM.ModelReference)a);
               }
            }
         }
         catch (System.Reflection.TargetException te)
         {
               ComApp.MessageCenter.AddMessage("Exception on the isIModel call", 
                  "The Has IModel code has failed to detect an i-model", 
                  BCOM.MsdMessageCenterPriority.Error, false);
         }
           
         //if it makes it here must not be an imodel
         return false;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// checks the active model
      /// does simple walk of  the  dgn file to look for schema that are not managed by the plugin.
      /// </summary>
      /// <param name="connection">The connection to  the  active dgn file.</param>
      /// <returns>true if a schema is found in model</returns>
/*-----------------------------------------------------------------------------*/
      public static bool FindEmbededSchemas(ECSR.RepositoryConnection connection,
                                            List<string> schemas)
      {
         bool bStatus = false;
         System.Collections.Generic.IList<ECOS.IECSchema> schemaList;
         schemaList = new System.Collections.Generic.List<ECOS.IECSchema>();
         ECPS.PersistenceService pService = ECPS.PersistenceServiceFactory.GetService();
         string[] schemaNames = pService.GetSchemaFullNames(connection);
         object schemaContext  = pService.GetSchemaContext(connection);

         bool bFirstOne = true;

         if ((schemaNames.Length <= 0) || (schemaNames.Length==2) && 
               IsOPMBaseSchema(schemaNames))
         {
            CSVReporter.writeLine(strCurrentProjectName, 
                                  BVSchemaChecker.ComApp.ActiveDesignFile.Name, 
                                  BVSchemaChecker.ComApp.ActiveModelReference.Name, 
                                  "", true);
            return bStatus;
         }
         else
         {
               //if the file is openned in OPM then OpenPlant and OpenPlant_3D are available by default. 
               //if OPM host then schemacontext.managed <3 or those not in there are are OP and OP3D
            bStatus = false;
            foreach (string fullSchemaName in schemaNames)
               {
                  ECOS.IECSchema schema = Bentley.ECObjects.ECObjects.LocateSchema(
                     fullSchemaName, ECOS.SchemaMatchType.LatestCompatible, null, schemaContext);
                  BDGNP.PersistenceStrategy pStrategy;
                  pStrategy = BDGNP.DgnECPersistence.GetPersistenceStrategiesForSchema(connection, schema);

                  if (bFirstOne)
                  {
                     bFirstOne = false;
                     if (bUseLogFile)
                           LogWriter.writeLine(
                              string.Format("FAILED: Processing File: {0} Model: {1}", 
                              BVSchemaChecker.ComApp.ActiveDesignFile.FullName, 
                              BVSchemaChecker.ComApp.ActiveModelReference.Name), true);
                  }
                  if ((!IsOnWhiteList(schema.Name)) && (!IsABDBaseSchema(schema.Name)) && 
                     (!IsOPMSchema(schema.Name)) && (!IsBRCMSchema(schema.Name)))
                  {
                     if ((s_bIsUSTN) || ((s_bIsOPM) && (((schema.Name.CompareTo("OpenPlant") != 0)
                                             && (schema.Name.CompareTo("OpenPlant_3D") != 0)))))
                     {
                        BVSchemaChecker.ComApp.MessageCenter.AddMessage(
                                 string.Format("found {0}_{1}_{2}", schema.Name,
                                 schema.VersionMajor, schema.VersionMinor),
                                 "found " + fullSchemaName, BCOM.MsdMessageCenterPriority.Info,
                              false);
                           bStatus = true;
                     }
                     if (bUseLogFile)
                           LogWriter.writeLine(string.Format("\t Found Schema {0} in the file", fullSchemaName), false);
                     //this is a FAILED line
                     if ((s_bIsUSTN) || ((s_bIsOPM) && (((schema.Name.CompareTo("OpenPlant") != 0)
                           && (schema.Name.CompareTo("OpenPlant_3D") != 0)))))
                     {
                        CSVReporter.writeLine(strCurrentProjectName,
                           BVSchemaChecker.ComApp.ActiveDesignFile.Name,
                           BVSchemaChecker.ComApp.ActiveModelReference.Name,
                           fullSchemaName, false);
                     }
                  }

                  if (!IsOnWhiteList(schema.Name) && 
                     !IsABDBaseSchema(schema.Name)&&!IsOPMSchema(schema.Name)&&
                     !IsBRCMSchema(schema.Name))
                     schemas.Add(fullSchemaName);
               }
         }
         return bStatus;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// processes a dgn model to see if it has embedded schema
      /// </summary>
      /// <param name="oModel">the model to check</param>
      /// <returns>true that the model was processed and does not have a schema.</returns>
/*-----------------------------------------------------------------------------*/
      public static bool ProcessModel(BCOM.ModelReference oModel)
      {
         List<string> schemaList = new List<string>();
         bool m_debugMode = false;
         bool m_silentMode = false;
         string errMessage = "ERROR";
         bool bStatus = true;

         ECSR.RepositoryConnection connection = null;
         CSVReporter.CreateReport(string.Format("SC_{0}.csv", DateTime.Now.ToString("yyyy-dd-MM")));
         if (m_debugMode)
               BVSchemaChecker.ComApp.MessageCenter.AddMessage("created csv file",
                  string.Format("created report file at {0}", CSVReporter.FileName),
                  BCOM.MsdMessageCenterPriority.Info, m_debugMode);
         try
         {
            ECSS.ECSession ecSession = ECSS.SessionManager.CreateSession(true);
            connection = BVSchemaChecker.OpenConnectionToModel(oModel,ecSession);

            if (BVSchemaChecker.FindEmbededSchemas(connection, schemaList))
               {
                  BVSchemaChecker.ComApp.MessageCenter.AddMessage("found schema in the file",
                                                                  "found Schema",
                                                                  BCOM.MsdMessageCenterPriority.Error,
                                                                  m_silentMode);
                  BVSchemaChecker.iSchemaFoundCount++;
                  SchemaListForm sform = new SchemaListForm();
                  sform.SetSchemaNames(schemaList);
                  sform.ShowDialog();
                  bStatus = false;
               }
            else
               BVSchemaChecker.ComApp.MessageCenter.AddMessage("no schema in the file",
                                                                  "nothing found",
                                                                  BCOM.MsdMessageCenterPriority.Info,
                                                                  m_silentMode);
         }
         catch (Exception e)
         {
               BVSchemaChecker.ComApp.MessageCenter.AddMessage(e.Message, e.Message,
                                                               BCOM.MsdMessageCenterPriority.Error,
                                                               !BVSchemaChecker.bUseLogFile);
               errMessage = e.Message;
               bStatus = false;
         }
         finally
         {
               if (null == connection)
                  CSVReporter.writeLine(BVSchemaChecker.strCurrentProjectName,
                                       BVSchemaChecker.ComApp.ActiveDesignFile.Name,
                                       BVSchemaChecker.ComApp.ActiveModelReference.Name,
                                       errMessage, false);
               else
                  BVSchemaChecker.CloseConnection(connection);

               CSVReporter.close();
         }
         return bStatus;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// This tries to remove the reference files so they are not loaded in the process.
      /// </summary>
/*-----------------------------------------------------------------------------*/
      public static void CleanOffRefFiles()
      {
         BCOM.ModelReference oModel = BVSchemaChecker.ComApp.ActiveModelReference;
         int iAttachmentCount = oModel.Attachments.Count;
         for (int i = iAttachmentCount; i > 0; --i)
         {
            BCOM.Attachment oAtt = oModel.Attachments[i];
            oModel.Attachments.Remove(oAtt);
         }

         return;
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// determine if the file is a dgn file.  We only process DGN files
      /// </summary>
      /// <param name="iProjectID">PW Project ID</param>
      /// <param name="iDocID">PW document ID</param>
      /// <returns>true if the file is a DGN</returns>
/*-----------------------------------------------------------------------------*/
      private static bool ProcessDocument(int iProjectID, int iDocID)
      {
         bool rtnValue = false;
         StringBuilder sbfname = new StringBuilder(512);

         int x = PWAPI.dmscli.aaApi_SelectDocument(iProjectID, iDocID);

         int iAppID = PWAPI.dmscli.aaApi_GetDocumentNumericProperty(
                              PWAPI.dmscli.DocumentProperty.ApplicationID, 0);
         int iFtype = PWAPI.dmscli.aaApi_GetDocumentNumericProperty(
                              PWAPI.dmscli.DocumentProperty.FileType, 0);
         PWAPI.dmscli.aaApi_GetDocumentFileName(iProjectID, iDocID, sbfname, 512);
         string strMimeType = PWAPI.dmscli.aaApi_GetDocumentStringProperty(
                                       PWAPI.dmscli.DocumentProperty.MimeType, 0);
         //tried this out on test repository nothing passed.
         // if (PWAPI.dmscli.FileType.DGNV8.Equals(iFtype)==false)
         //this mime type looks to be good for all dgn files...
         if (!strMimeType.Contains("image/vnd.dgn;ver=8"))
               return false;

         int iModelext = sbfname.ToString().IndexOf(".i.dgn");
         string fileExt = Path.GetExtension(sbfname.ToString());
         string dgnExt = Path.GetExtension(sbfname.ToString());

         return (dgnExt.CompareTo(".dgn") == 0) && (iModelext <= 0);
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// Processes the contents of a directory.  
      /// Checks out each file and tests for presence of embeded schema
      /// </summary>
      /// <param name="iProjectID">The PW Project ID</param>
/*-----------------------------------------------------------------------------*/
      public static void BVSchemaCheckerProcessDocuments(int iProjectID)
      {
         int docCount = PWAPI.dmscli.aaApi_SelectDocumentsByProjectId(iProjectID);

         if (docCount <= 0)
            return;

         //docCount = PWAPI.dmscli.aaApi_GetDocumentCount(iProjectID);
         for (int i = 0; i < docCount; ++i)
         {
            PWAPI.dmscli.aaApi_SelectDocumentsByProjectId(iProjectID);
            int did = PWAPI.dmscli.aaApi_GetDocumentId(i);
        
               //appidd can be different on different installations... need to make this better.
            if (ProcessDocument(iProjectID,did))
               {
                  StringBuilder sbWorkingFileName = new StringBuilder(512);
                  if (did > 0)
                     if (PWAPI.dmscli.aaApi_CopyOutDocument(iProjectID, did, 
                                                   null, sbWorkingFileName, 512))
                     {
                           //this opens the file
                        BVSchemaChecker.ComApp.OpenDesignFile(
                                                   sbWorkingFileName.ToString(), 
                                                   true, BCOM.MsdV7Action.Workmode);
                           //this will  loop through the models in the file
                        if (!s_processAllModels)
                           {
                              CleanOffRefFiles();
                              KeyinCommands.BVSchemaCheckerCommand("");
                           }
                           else
                              foreach (BCOM.ModelReference oModel in 
                                 BVSchemaChecker.ComApp.ActiveDesignFile.Models)
                              {
                                 oModel.Activate();
                                 //this is to remove the reference files
                                 CleanOffRefFiles();
                                 //this checks the schema
                                 KeyinCommands.BVSchemaCheckerCommand("");
                              }

                        iFileCount++; //the file has been checked.
                        PWAPI.dmscli.aaApi_CheckInDocument(iProjectID, did);
                     }
                     else
                     {
                        string errMessage = PWAPI.dmsgen.aaApi_GetLastErrorMessage();
                        string errDetail = PWAPI.dmsgen.aaApi_GetLastErrorDetail();
                        BVSchemaChecker.ComApp.MessageCenter.AddMessage(
                              errMessage, errDetail, BCOM.MsdMessageCenterPriority.Info, false);
                     }
             }
         }
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// recursively walks the  project hierarchy
      /// </summary>
      /// <param name="iProjectID">the project to process</param>
/*-----------------------------------------------------------------------------*/
      public static void BVSchemaCheckerTraverseProject(int iProjectID)
      {
         //need to find a way to check  to make sure it is a dgn file that is better than just the extension...
         StringBuilder sbBuffer = new StringBuilder(1024);
         PWAPI.dmscli.aaApi_GetProjectNamePath2(iProjectID, false, '-', sbBuffer, 1024);
         strCurrentProjectName = sbBuffer.ToString();

         BVSchemaCheckerProcessDocuments(iProjectID);
   
         int numChildProjects;
         IntPtr HAADMSBUFFER_SubProjects;
         HAADMSBUFFER_SubProjects = PWAPI.dmscli.aaApi_SelectProjectDataBufferChilds(iProjectID);
         numChildProjects = PWAPI.dmscli.aaApi_DmsDataBufferGetCount(HAADMSBUFFER_SubProjects);
         for (int iSubCount = 0; iSubCount < numChildProjects; ++iSubCount)
         {
            int childProjectID = PWAPI.dmscli.aaApi_DmsDataBufferGetNumericProperty(
                                             HAADMSBUFFER_SubProjects, 1, iSubCount);
            BVSchemaCheckerTraverseProject(childProjectID);
         }
         //PWAPI.dmscli.aaApi_DmsDataBufferFree(HAADMSBUFFER_project);
         PWAPI.dmscli.aaApi_DmsDataBufferFree(HAADMSBUFFER_SubProjects);
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// calls the check method on the close file event.
      /// </summary>
      /// <param name="sender">the class that originated the event</param>
      /// <param name="eventArgs">the arguments to the event.  this will be the 
      /// time (bfore or after)</param>
/*-----------------------------------------------------------------------------*/
      internal static void BVSchemFileEventHandler(Bentley.MicroStation.AddIn sender, NewDesignFileEventArgs eventArgs)
      {
         if ((eventArgs.WhenCode == NewDesignFileEventArgs.When.BeforeDesignFileClose) && (!s_runningTraverse))
            KeyinCommands.BVSchemaCheckerCommand("FROM_HOOK");
      }

/*-----------------------------------------------------------------------------*/
      /// <summary>
      /// called on the element to file event.  
      /// removed in favor of the native hook
      /// </summary>
      /// <param name="sender">The object that caused this event.</param>
      /// <param name="args">the type of change.</param>
/*-----------------------------------------------------------------------------*/
      internal static void BVSchemaElementEventHandler(Bentley.MicroStation.AddIn sender, ElementChangedEventArgs args)
      {
      //   Console.WriteLine("Element changed " + args.GetType().ToString());
      /*   if(null != args.NewElement)
               Debug.Print("Element being added is " + args.NewElement.Type.ToString());
         if (0 != (long)args.OldElementRef)
               Debug.Print("the old element is of type " + elementRef_getType((long)args.OldElementRef).ToString());
         */ 
      }
      }   // End of BVSchemaChecker
}   // End of Namespace
