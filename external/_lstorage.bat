@echo off
setlocal enabledelayedexpansion
rem "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\xsd.exe"
rem C:\Users\V.Devgutis\AppData\Local\Packages\1883eea9-971b-4294-8dc0-03e438fab041_er8rbdqtt6q58\LocalState\ 
rem C:\Users\V.Devgutis\AppData\Local\Packages\1883eea9-971b-4294-8dc0-03e438fab041_er8rbdqtt6q58\LocalState\Lex.Db\msdnus_db
rem C:\Users\V.Devgutis\AppData\Local\Packages\6ecd9860-2bf4-4d2c-b6fe-22df80708c5c_er8rbdqtt6q58\ 

App:
"C:\Users\V.Devgutis\AppData\Local\Packages\21313vdasus.msdnus_8k9evrm7qaqe4\LocalState\Microsoft\Windows Store\ApiData\" 

Tests: 
C:\Users\V.Devgutis\AppData\Local\Packages\1883eea9-971b-4294-8dc0-03e438fab041_er8rbdqtt6q58\LocalState

set FIL="D:\REPO\VDG\Mobile\msdnus\temp\KeysExport.xml"
set BIN="C:\Users\V.Devgutis\AppData\Local\Packages\1883eea9-971b-4294-8dc0-03e438fab041_er8rbdqtt6q58\LocalState\"

xcopy /Y !FIL! !BIN!

