@echo off

rem Set default environment variables
if not defined UNITY_EDITOR_PATH set "UNITY_EDITOR_PATH=C:\Program Files\Unity\Hub\Editor\2021.3.22f1\Editor"
if not defined UNITY_BUILD_PATH set "UNITY_BUILD_PATH=Build"
if not defined LOGFILE_PATH set "LOGFILE_PATH=%UNITY_BUILD_PATH%\build.log"

rem Run Unity in batch mode
"%UNITY_EDITOR_PATH%\Unity.exe" -batchmode -quit -projectPath "%cd%" -buildTarget Android -executeMethod AndroidBuilder.Build -logfile "%LOGFILE_PATH%" -outputPath="%UNITY_BUILD_PATH%" %*

rem List and echo the generated APK path
for /r "%UNITY_BUILD_PATH%" %%f in (*.apk) do (
    set APK_PATH=%%f
)

echo Generated APK: %APK_PATH%
