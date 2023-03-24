@echo off
echo =============================================
cd "C:\Program Files (x86)\Steam\"
call :waitforgame
steam -applaunch 255710 -unsubscribe
call :waitforgame
steam -applaunch 255710 -subscribe


:waitforgame
	echo waiting for CS to terminate ...
	> nul timeout /t:1
	for /f "tokens=1 delims= " %%A in ('tasklist /fi ^"Imagename eq Cities.exe^" ^| find ^"Cities.exe^"') do GOTO waitforgame

exit /b