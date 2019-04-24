FROM mcr.microsoft.com/dotnet/core/runtime:2.2

COPY ./endgame-poc/bin/Release/netcoreapp2.2/publish /endgame-poc

ENTRYPOINT ["dotnet", "endgame-poc/endgame-poc.dll"]
