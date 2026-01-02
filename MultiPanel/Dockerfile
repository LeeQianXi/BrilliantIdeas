FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 复制每个项目的 .csproj 文件
COPY ["MultiPanel.Abstractions/MultiPanel.Abstractions.csproj", "MultiPanel.Abstractions/"]
COPY ["MultiPanel.Grains/MultiPanel.Grains.csproj", "MultiPanel.Grains/"]
COPY ["MultiPanel.Interfaces/MultiPanel.Interfaces.csproj", "MultiPanel.Interfaces/"]
COPY ["MultiPanel.Shared/MultiPanel.Shared.csproj", "MultiPanel.Shared/"]
COPY ["MultiPanel.Silo/MultiPanel.Silo.csproj", "MultiPanel.Silo/"]

# 恢复依赖
RUN dotnet restore "MultiPanel.Silo/MultiPanel.Silo.csproj"

# 复制所有源代码
COPY [".", "."]

# 构建
WORKDIR "/src/MultiPanel.Silo"
RUN dotnet build "MultiPanel.Silo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MultiPanel.Silo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiPanel.Silo.dll"]