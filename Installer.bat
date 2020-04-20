@echo off
set oname=netcoreapp3.1
for /f "delims=" %%i in ("%cd%") do set name=%%~ni
cd bin\Release
rename "%oname%" "%name%"
"C:\Program Files\7-Zip\7z.exe" a -sfx7z.sfx "..\..\%name%[sfx].exe" "%name%" -mx
rename "%name%" "%oname%"
"D:\Someone\upx\upx.exe" --best "..\..\%name%[sfx].exe"
pause