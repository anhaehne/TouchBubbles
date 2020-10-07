ARG BUILD_FROM

# Build environment
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish src/TouchBubbles/Server -c Release -o out

# Runtime environment
FROM $BUILD_FROM
ENV LANG C.UTF-8

# Install .net core dependencies
RUN apk add --no-cache libintl
RUN apk add --no-cache icu
RUN apk add --no-cache zlib
RUN apk add --no-cache libcurl

# Install .net core 
RUN wget https://dot.net/v1/dotnet-install.sh
RUN bash dotnet-install.sh -c 3.1 --runtime aspnetcore

# Copy build result
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS http://+:8099;http://+:9025
ENTRYPOINT ["/root/.dotnet/dotnet", "TouchBubbles.Server.dll"]

LABEL io.hass.version="VERSION" io.hass.type="addon" io.hass.arch="armhf|aarch64|i386|amd64"