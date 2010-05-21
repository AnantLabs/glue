@echo off
set frameworkdir=%windir%\Microsoft.NET\Framework\v2.0.50727
%frameworkdir%\msbuild /p:Configuration=Release /t:rebuild
