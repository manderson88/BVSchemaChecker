REM Use a postbuild event like:
REM
REM call $(ProjectDir)StandardPostBuild.bat $(TargetPath) VALIDATEADDIN
REM 
REM %1 is $(TargetPath) -- path and name of DLL
REM %2 VALIDATEADDIN option
REM

@echo on
set MS=C:\PROGRA~2\Bentley\MICROS~2\MICROS~1\
if NOT exist "%MS%\ustation.exe" goto invalidMSPath

set MSMDE=C:\Progra~2\Bentley\MICROS~1\MICROS~1\
if NOT exist "%MSMDE%\MDL\BIN\UstnXOM.exe" goto invalidMSPath 

if ".%2" == ".VALIDATEADDIN" "%MSMDE%\MDL\BIN\UstnXOM" ValidateAddIn "%1"

rem uncomment this to copy the commands.xml file to the ouput.  In C# and VB you can embed this
rem file in the dll through the IDE so you do not have to copy
rem echo %4
rem copy commands.xml %4\

set _ConfigFilename="%MS%\Config\appl\%~n1_Config.cfg"

echo  ** Creating %_ConfigFilename% so that the AddIn can be found by MicroStation **
echo # > %_ConfigFilename%
echo #  If this is placed into $(MS)/Config/appl then it should	>> %_ConfigFilename%
echo #  be possible to do "mdl load" on this sample AddIn.>> %_ConfigFilename%
echo #>> %_ConfigFilename%
echo MS_ADDINPATH ^> %~d1%~p1 >> %_ConfigFilename%

exit /b 0

:invalidMSPath
echo "Invalid path to Microstation"
exit 1