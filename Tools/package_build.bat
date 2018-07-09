@echo off

if not defined MAFIAUNITY_BUILD_DIR (
	echo MAFIAUNITY_BUILD_DIR has to be defined!
	exit
)

echo Copying Base mod to the game directory...

mkdir %MAFIAUNITY_BUILD_DIR%\Mods

xcopy /e/y ..\Mods\MafiaBase %MAFIAUNITY_BUILD_DIR%\Mods\MafiaBase\

echo Copying other metadata...

xcopy /e/y ..\Docs %MAFIAUNITY_BUILD_DIR%\Docs\

xcopy /y ..\LICENSE %MAFIAUNITY_BUILD_DIR%\
xcopy /y ..\NOTICE %MAFIAUNITY_BUILD_DIR%\
xcopy /y ..\AUTHORS.md %MAFIAUNITY_BUILD_DIR%\
xcopy /y ..\README.md %MAFIAUNITY_BUILD_DIR%\