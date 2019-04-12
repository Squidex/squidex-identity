#
# Stage 1, Prebuild
#
FROM microsoft/dotnet:2.2-sdk as builder

WORKDIR /src

COPY . .

# Restore Dependencies
RUN dotnet restore

# Publish
RUN dotnet publish Squidex.Identity/Squidex.Identity.csproj --output /out/ --configuration Release