cd ServerConsole/bin/Debug/
START ServerConsole.exe --port 1234 --conf Config/ServerSettings.xml
cd ../../../GameMaster/bin/Debug/
START GameMaster.exe --address 192.168.0.16 --port 1234 --conf Config/GameMasterSettings.xml
cd ../../../Player/bin/Debug/

IF "%1"=="" set a=1 
IF not "%1"=="" set a=%1

for /l %%i in (1,1,%a%) do START Player.exe --address 192.168.0.16 --port 1234 --conf Config/PlayerSettings.xml --game "Initial game" --team blue --role player
