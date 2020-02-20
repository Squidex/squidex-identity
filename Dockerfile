#
# Stage 1, Prebuild
#
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster as backend

WORKDIR /src

COPY . .

# Restore Dependencies
RUN dotnet restore

# Publish
RUN dotnet publish Squidex.Identity/Squidex.Identity.csproj --output /build/ --configuration Release

#
# Stage 2, Build runtime
#
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim

# Default AspNetCore directory
WORKDIR /app

# Copy from build stage
COPY --from=backend /build/ .

EXPOSE 80

ENTRYPOINT ["dotnet", "Squidex.Identity.dll"]