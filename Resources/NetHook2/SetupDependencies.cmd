@echo off
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0\InstallDependencies.ps1"

pushd .
cd %~dp0
call BuildDependencies.cmd
popd