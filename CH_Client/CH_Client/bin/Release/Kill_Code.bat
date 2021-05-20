%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c %~s0 ::","","runas",1)(window.close)&&exit
@echo off

timeout /t 1
echo.
echo Kill Code Start

del %~dp0CH_Client.exe
