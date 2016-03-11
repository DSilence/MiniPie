@ECHO OFF &SETLOCAL
set version=0.0.2.0
IF NOT [%1] == [] (
		set version=%1
	)


cd MiniPie
SET "file=MiniPie.nuspec"
SET /a Line#ToSearch=5
SET "Replacement=        <version>%version%</version>"

(FOR /f "tokens=1*delims=:" %%a IN ('findstr /n "^" "%file%"') DO (
    SET "Line=%%b"
    IF %%a equ %Line#ToSearch% SET "Line=%Replacement%"
    SETLOCAL ENABLEDELAYEDEXPANSION
    ECHO(!Line!
    ENDLOCAL
))>"%file%.new"
MOVE "%file%.new" "%file%"

nuget pack MiniPie.nuspec

for /r "..\packages" %%a in ('DIR *.* /B /O:-D') do (
	if "%%~nxa"=="Squirrel.exe" set sq=%%~dpnxa
	)

%sq% --releasify MiniPie.%version%.nupkg -g .\Images\Loading.gif --setupIcon App.ico --no-msi
MOVE "Releases\Setup.exe" "Releases\MiniPieSetup.exe"