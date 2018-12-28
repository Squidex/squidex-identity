#
# Stage 1, Prebuild
#
FROM microsoft/dotnet:2.1-sdk as builder

WORKDIR /src

COPY . .
 
# Test Backend
RUN dotnet restore

# Publish
RUN dotnet publish Squidex.Identity/Squidex.Identity.csproj --output /out/ --configuration Release

#
# Stage 2, Build runtime
#
FROM microsoft/dotnet:2.1.0-aspnetcore-runtime

# Default AspNetCore directory
WORKDIR /app

# Copy from build stage
COPY --from=builder /out/ .

EXPOSE 80

ENTRYPOINT ["dotnet", "Squidex.Identity.dll"]