#!/bin/bash

dotnet publish --runtime linux-x64 ./Core/Server/Server.csproj
dotnet publish --runtime linux-arm64 ./Core/Server/Server.csproj
dotnet publish --runtime win-x64 ./Core/Server/Server.csproj
dotnet publish --runtime win-arm64 ./Core/Server/Server.csproj

zip -r ./linux-x64.zip ./Core/Server/bin/Release/net10.0/linux-x64
zip -r ./linux-arm64.zip ./Core/Server/bin/Release/net10.0/linux-arm64
zip -r ./win-x64.zip ./Core/Server/bin/Release/net10.0/win-x64
zip -r ./win-arm64.zip ./Core/Server/bin/Release/net10.0/win-arm64