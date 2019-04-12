#
# Stage 1, Prebuild
#
FROM microsoft/dotnet:2.2-sdk as builder

WORKDIR /src

COPY . .

# Restore Dependencies
RUN dotnet restore

# Publish
RUN dotnet publish Squidex.Identity/Squidex.Identity.csproj --output /out/alpine --configuration Release -r alpine.3.7-x64

#
# Stage 2, Build runtime
#
FROM microsoft/dotnet:2.2-runtime-deps-alpine

# Default AspNetCore directory
WORKDIR /app

# add libuv
RUN apk add --no-cache libuv \
 && ln -s /usr/lib/libuv.so.1 /usr/lib/libuv.so

# Copy from build stage
COPY --from=builder /out/alpine .

EXPOSE 80

ENTRYPOINT ["./Squidex.Identity"]