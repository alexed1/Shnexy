@echo off

".nuget/nuget.exe" restore Shnexy.sln
if not "%ERRORLEVEL%" == "0" goto :ERROR
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" Kwasant.sln
if not "%ERRORLEVEL%" == "0" goto :ERROR
"packages\NUnit.Runners.2.6.3\tools\nunit-console.exe" Tests/KwasantTest/KwasantTest.csproj
if not "%ERRORLEVEL%" == "0" goto :ERROR

exit /B 0

:ERROR
exit /B 1