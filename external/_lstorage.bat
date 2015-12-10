@echo off
setlocal enabledelayedexpansion
rem "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\xsd.exe" 
rem cd C:\Users\V.Devgutis\AppData\Local\Packages\1883eea9-971b-4294-8dc0-03e438fab041_er8rbdqtt6q58\LocalState\ 
rem cd C:\Users\V.Devgutis\AppData\Local\Packages\1883eea9-971b-4294-8dc0-03e438fab041_er8rbdqtt6q58\LocalState\Lex.Db\msdnus_db

set FIL="D:\REPO\VDG\Mobile\msdnus\temp\KeysExport.xml"
set BIN="C:\Users\V.Devgutis\AppData\Local\Packages\1883eea9-971b-4294-8dc0-03e438fab041_er8rbdqtt6q58\LocalState\"

xcopy /Y !FIL! !BIN!

