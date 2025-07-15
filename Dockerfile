#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# -------- Base Runtime Image --------
# This is the lightweight image used to run the app (no SDK tools, just runtime)
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

# These ports are exposed for HTTP (80) and HTTPS (443)
EXPOSE 80
EXPOSE 443

# -------- Build Image --------
# This image includes the full .NET SDK for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy project files for all layers (PL = API, BLL = Business Logic, DAL = Data Access)
COPY ["POS_Server/POS_Server_PL/POS_Server_PL.csproj", "POS_Server/POS_Server_PL/"]
COPY ["POS_Server/POS_Server_BLL/POS_Server_BLL.csproj", "POS_Server/POS_Server_BLL/"]
COPY ["POS_Server/POS_Server_DAL/POS_Server_DAL.csproj", "POS_Server/POS_Server_DAL/"]

# Copy project files for external projects (POS_Domains & Helper)
COPY ["Helper/Helper/Helper.csproj", "Helper/Helper/"]
COPY ["POS_Domains/POS_Domains/POS_Domains.csproj", "POS_Domains/POS_Domains/"]

# Restore NuGet packages
RUN dotnet restore "POS_Server/POS_Server_PL/POS_Server_PL.csproj"

# Copy all source code
COPY . .

# Set working directory to the API project and build it
WORKDIR "/src/POS_Server/POS_Server_PL"
RUN dotnet build "POS_Server_PL.csproj" -c Release -o /app/build

# -------- Publish Image --------
# This stage publishes the app to a folder (trims build artifacts, prepares for runtime)
FROM build AS publish
RUN dotnet publish "POS_Server_PL.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------- Final Runtime Image --------
# This stage runs the app using only the runtime image (smallest size)
FROM base AS final
WORKDIR /app

# Add this so the app listens on all network interfaces inside Docker
ENV ASPNETCORE_URLS=http://+:80

# Copy published output from previous stage
COPY --from=publish /app/publish .

# Set the entry point to run your Web API
ENTRYPOINT ["dotnet", "POS_Server_PL.dll"]
