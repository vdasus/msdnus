@echo off
setlocal enabledelayedexpansion
rem "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\xsd.exe" 

set FIL="D:\REPO\VDG\Mobile\msdnus\temp\KeysExport.xml"
set BIN="D:\REPO\VDG\Mobile\msdnus\msdnus\bin\x86\Debug\"

xcopy /Y !FIL! !BIN!

