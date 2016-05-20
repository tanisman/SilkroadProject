# SilkroadProject
Silkroad Online Emulator for Open Beta Client

### Requirements
- Visual Studio 2015
- Microsoft SQL Server 2008 or above
- Asio 1.10.6 for SCore (our AsyncTimer uses asio)

### How To Setup Server
- Restore database dumps
- Edit _ServerConfig table in SR_Global
- Edit GatewayServer/Data/Globals.cs and SR_GameServer/Data/Globals.cs for connection strings
- Compile project with visual studio and run

### How To Setup Client
- Open Media.pk2 with pk2 extractor and export GATEIP.txt
- Open GATEIP.txt with a hex editor and edit ip address (fill with zeroes remaining)
- Open client with IP Input change ip address again (aka change DIVISIONINFO.txt)

### How To Contribute Project
- You can make pull requests to contribute project.
  1. Your code must match projects design.
  2. Prevent deadlocks when interacting another client.
  3. Use Interlocked operations when accessing volatile variables
  4. Use async Task<T> for heavy operations
  5. Test your code carefully.

### Links for projects
Description | Link
------------ | -------------
Database Dumps | https://mega.nz/#!UAIRWJTK!66r4XsTFAbfazIP5CvrYqQYHGyuXpLWyaH36cNWVRRc
Client | https://mega.nz/#!sRBiBaLY!RQnCfHfr8HfBIRxRJctf0-5_MVC8W8OXd51pH558mm8
Client Dll (Aqua.dll) source | https://mega.nz/#!MAZQUZpY!4frt9k4PDnDVFvrV02KBbx4K0xCcnZ2aUZ5W4uSfeTo
