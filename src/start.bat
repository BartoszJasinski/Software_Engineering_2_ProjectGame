cd ServerConsole/bin/Debug/
START ServerConsole.exe --port 1234 --conf Config/ServerSettings.xml
cd ../../../GameMaster/bin/Debug/
START GameMaster.exe --address 192.168.0.103 --port 1234 --conf Config/GameMasterSettings.xml
cd ../../../Player/bin/Debug/
ping -n 2 127.0.0.1 > nul
IF "%1"=="" set a=1
ELSE set a=%1
ECHO %a%
for /l %%i in (1,1,%a%) do START Player.exe --address 192.168.0.103 --port 1234 --conf Config/PlayerSettings.xml --game "Initial game" --team blue --role player
