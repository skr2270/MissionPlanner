REM ********* this VB Script is a wrapper for the driver install executables
REM ********* it will launch the application with Windows focus so it doesn't 'pop under' out of view

Function launch()

   Dim WshShell

   Set WshShell = CreateObject("WScript.Shell")

   WshShell.Run """C:\Program Files\UAV Solutions\MissionPlanner\Drivers\DPInstx86.exe""", 1, true

End function