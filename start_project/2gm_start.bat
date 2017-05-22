SET ip=192.168.0.16

cd ../src/ServerConsole/bin/Debug/
START ServerConsole.exe -p 5000 -c Config/ServerSettings.xml
cd ../../../GameMaster/bin/Debug/
START GameMaster.exe -a %ip% -p 5000 -c Config/GameMasterSettings.xml
START GameMaster.exe -a %ip% -p 5000 -c Config/GameMasterSettings2.xml
cd ../../../Player/bin/Debug/

IF "%1"=="" set a=1 
IF not "%1"=="" set a=%1

for /l %%i in (1,1,%a%) do START Player.exe -a %ip% -p 5000 -c Config/PlayerSettings.xml -n "Initial game" -t blue -r player
for /l %%i in (1,1,%a%) do START Player.exe -a %ip% -p 5000 -c Config/PlayerSettings.xml -n "Initial game2" -t blue -r player
cd ../../..