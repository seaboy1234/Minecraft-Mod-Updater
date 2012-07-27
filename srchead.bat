@ECHO OFF
IF NOT EXIST "SrcHead" GOTO NOEXIST
START "" /Wait /Min "SrcHead\SrcHead.exe" "%CD%"
EXIT
:NOEXIST
ECHO SrcHead was not found.  Oh well.