@echo off
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0\InstallDependencies.ps1"

call BuildDependencies.cmd