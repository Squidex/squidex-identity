#
# Stage 1, Prebuild
#
FROM microsoft/dotnet:2.1-sdk as builder

COPY . .

WORKDIR /
 
# Test Backend
RUN dotnet restore

# Publish
RUN dotnet publish Squidex.Identity/Squidex.Identity.csproj --output /out/ --configuration Release

#
# Stage 2, Build runtime
#
FROM microsoft/dotnet:2.1-runtime-deps-alpine

# Default AspNetCore directory
WORKDIR /app

# add libuv
RUN apk add --no-cache libuv \
&& ln -s /usr/lib/libuv.so.1 /usr/lib/libuv.so


# Copy from build stage
COPY --from=builder /out/ .

EXPOSE 80

ENTRYPOINT ["dotnet", "Squidex.Identity.dll"]